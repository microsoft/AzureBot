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
                Assert.IsTrue(reply.Contains(step.ExpectedReply), step.ErrorMessageHandler(step.Action, step.ExpectedReply, reply));
                
                if (step.Verified != null)
                {
                    step.Verified(reply);
                }
            }

            if (completionTestCases != null && completionTestCases.Any())
            {
                Action<IList<string>> action = (replies) =>
                {
                   var singleCompletionTestCase = completionTestCases.Count == 1;

                    for (int i = 0; i < completionChecks; i++)
                    {
                        var completionIndex = singleCompletionTestCase ? 0 : i;
                        var completionTestCase = completionTestCases[completionIndex];

                        Assert.IsTrue(
                            replies[i].Contains(completionTestCase.ExpectedReply),
                            completionTestCase.ErrorMessageHandler(completionTestCase.Action, completionTestCase.ExpectedReply, replies[i]));

                        if (completionTestCase.Verified != null)
                        {
                            completionTestCase.Verified(replies[i]);
                        }
                    }
                };

                await General.BotHelper.WaitForLongRunningOperations(action, completionChecks);
            }
        }
    }
}
