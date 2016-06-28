namespace AzureBot.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    internal class TestRunner
    {
        internal static async Task RunTestCase(BotTestCase testCase)
        {
            await RunTestCases(new List<BotTestCase> { testCase });
        }

        internal static async Task RunTestCases(IList<BotTestCase> steps, BotTestCase completionTestCase = null, int completionChecks = 1)
        {
            foreach (var step in steps)
            {
                var reply = await General.BotHelper.SendMessage(step.Action);
                Assert.IsTrue(reply.Contains(step.ExpectedReply), step.ErrorMessageHandler(reply, step.ExpectedReply));
            }

            if (completionTestCase != null)
            {
                Action<IList<string>> action = (replies) =>
                {
                    for (int i = 0; i < completionChecks; i++)
                    {
                        Assert.IsTrue(
                            replies[0].Contains(completionTestCase.ExpectedReply), 
                            completionTestCase.ErrorMessageHandler(replies[0], completionTestCase.ExpectedReply));
                    }
                };

                await General.BotHelper.WaitForLongRunningOperations(action, completionChecks);
            }
        }
    }
}
