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

        internal static async Task RunTestCases(IList<BotTestCase> testCases, BotTestCase completionTestCase = null)
        {
            foreach (var testCase in testCases)
            {
                var reply = await General.BotHelper.SendMessage(testCase.Action);
                Assert.IsTrue(reply.Contains(testCase.ExpectedReply), testCase.ErrorMessageHandler(reply, testCase.ExpectedReply));
            }

            if (completionTestCase != null)
            {
                Action<string> action = (reply) =>
                {
                    Assert.IsTrue(reply.Contains(completionTestCase.ExpectedReply), completionTestCase.ErrorMessageHandler(reply, completionTestCase.ExpectedReply));
                };

                await General.BotHelper.WaitForLongRunningOperation(action);
            }
        }
    }
}
