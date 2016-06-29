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

        internal static async Task RunTestCases(IList<BotTestCase> steps, IList<BotTestCase> completionTestCases = null, int completionChecks = 1)
        {
            if (completionTestCases != null && completionTestCases.Count > 1 && completionTestCases.Count < completionChecks)
            {
                Assert.Fail($"There are completion test cases missing. Completion Test Cases: {completionTestCases.Count} for {completionChecks} completionChecks");
            }

            foreach (var step in steps)
            {
                var reply = await General.BotHelper.SendMessage(step.Action);
                Assert.IsTrue(reply.Contains(step.ExpectedReply), step.ErrorMessageHandler(reply, step.ExpectedReply));
            }

            if (completionTestCases != null && completionTestCases.Any())
            {
                Action<IList<string>> action = (replies) =>
                {
                   var singleCompletionTestCase = completionTestCases.Count == 1;

                    for (int i = 0; i < completionChecks; i++)
                    {
                        var completionIndex = singleCompletionTestCase ? 0 : i;

                        Assert.IsTrue(
                            replies[i].Contains(completionTestCases[completionIndex].ExpectedReply),
                            completionTestCases[completionIndex].ErrorMessageHandler(replies[i], completionTestCases[completionIndex].ExpectedReply));
                    }
                };

                await General.BotHelper.WaitForLongRunningOperations(action, completionChecks);
            }
        }
    }
}
