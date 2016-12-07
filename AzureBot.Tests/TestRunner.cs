namespace AzureBot.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    internal class TestRunner
    {
        internal static async Task RunTestCase(BotTestCase testCase)
        {
            await RunTestCases(new List<BotTestCase> { testCase }, new List<BotTestCase>());
        }

        internal static async Task RunTestCases(IList<BotTestCase> steps, BotTestCase completionTestCase = null, int completionChecks = 1)
        {
            await RunTestCases(steps, new List<BotTestCase> { completionTestCase }, completionChecks);
        }

        internal static async Task RunTestCases(IList<BotTestCase> steps, IList<BotTestCase> completionTestCases = null, int completionChecks = 1, bool strictCheck = true)
        {
            if (completionTestCases != null && completionTestCases.Count > 1 && completionTestCases.Count < completionChecks)
            {
                Assert.Fail($"There are completion test cases missing. Completion Test Cases: {completionTestCases.Count} for {completionChecks} completionChecks");
            }

            foreach (var step in steps)
            {
                await General.BotHelper.SendMessageNoReply(step.Action);

                Action<IList<string>> action = (replies) =>
                {
                    var match = replies.FirstOrDefault(stringToCheck => stringToCheck.ToLowerInvariant().Contains(step.ExpectedReply));
                    Assert.IsTrue(match != null, step.ErrorMessageHandler(step.Action, step.ExpectedReply, string.Join(", ", replies)));
                    step.Verified?.Invoke(replies.LastOrDefault());
                };
                await General.BotHelper.WaitForLongRunningOperations(action, 1);
            }

            if (completionTestCases != null && completionTestCases.Any())
            {
                Action<IList<string>> action = (replies) =>
                {
                   var singleCompletionTestCase = completionTestCases.Count == 1;

                    for (int i = 0; i < replies.Count(); i++)
                    {
                        if (!strictCheck && completionChecks > replies.Count())
                        {
                            var skip = completionChecks - replies.Count();

                            completionTestCases = completionTestCases.Skip(skip).ToList();
                        }

                        var completionIndex = singleCompletionTestCase ? 0 : i;
                        var completionTestCase = completionTestCases[completionIndex];

                        Assert.IsTrue(
                            replies[i].Contains(completionTestCase.ExpectedReply.ToLowerInvariant()),
                            completionTestCase.ErrorMessageHandler(completionTestCase.Action, completionTestCase.ExpectedReply, replies[i]));

                        completionTestCase.Verified?.Invoke(replies[i]);
                    }
                };

                await General.BotHelper.WaitForLongRunningOperations(action, completionChecks);
            }
        }
    }
}
