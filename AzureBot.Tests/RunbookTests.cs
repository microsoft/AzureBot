namespace AzureBot.Tests
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Tests all VM commands
    /// </summary>
    [TestClass]
    public class RunbookTests
    {
        [TestMethod]
        [TestCategory("Parallel")]
        public async Task ShoudListRunbooks()
        {
            var testCase = new BotTestCase()
            {
                Action = "list my runbooks",
                ExpectedReply = "Available runbooks are",
                ErrorMessageHandler = (message, expected) => $"List runbooks failed with message: '{message}'. The expected message is '{expected}'."
            };

            await TestRunner.RunTestCase(testCase);
        }

        [TestMethod]
        [TestCategory("Parallel")]
        public async Task ShoudListAutomationAccounts()
        {
            var testCase = new BotTestCase()
            {
                Action = "list automation accounts",
                ExpectedReply = "Available automations accounts are",
                ErrorMessageHandler = (message, expected) => $"List automation accounts failed with message: '{message}'. The expected message is '{expected}'."
            };

            await TestRunner.RunTestCase(testCase);
        }

        [TestMethod]
        [TestCategory("Parallel")]
        public async Task RunRunbookShouldNotifyWhenTheSpecifiedRunbookDoesNotExists()
        {
            var runbook = "notfoundRunbook";

            var testCase = new BotTestCase()
            {
                Action = $"run runbook {runbook}",
                ExpectedReply = $"The '{runbook}' runbook was not found in any of your automation accounts.",
                ErrorMessageHandler = (message, expected) => $"Run runbook failed with message: '{message}'. The expected message is '{expected}'."
            };

            await TestRunner.RunTestCase(testCase);
        }

        [TestMethod]
        [TestCategory("Parallel")]
        public async Task ShowJobOutputShouldNotifyWhenTheSpecifiedJobDoesNotExists()
        {
            var jobId = "job5";

            var testCase = new BotTestCase()
            {
                Action = $"show {jobId} output",
                ExpectedReply = $"The job with id '{jobId}' was not found.",
                ErrorMessageHandler = (message, expected) => $"Show job output failed with message: '{message}'. The expected message is '{expected}'."
            };

            await TestRunner.RunTestCase(testCase);
        }

        [TestMethod]
        [TestCategory("Parallel")]
        public async Task ShowJobOutputShouldNotifyWhenAJobIsNotSpecified()
        {
            var testCase = new BotTestCase()
            {
                Action = $"show output",
                ExpectedReply = "No runbook job id was specified. Try 'show <jobId> output'.",
                ErrorMessageHandler = (message, expected) => $"Show job output failed with message: '{message}'. The expected message is '{expected}'."
            };

            await TestRunner.RunTestCase(testCase);
        }
    }
}
