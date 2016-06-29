namespace AzureBot.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Tests all VM commands
    /// </summary>
    [TestClass]
    public class RunbookTests
    {
        public TestContext TestContext { get; set; }

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
        public async Task ShowJobOutputShouldNotifyWhenTheSpecifiedJobDoesNotExist()
        {
            var jobId = "job0";

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
        public async Task ShowJobOutputShouldNotifyWhenJobIsNotSpecified()
        {
            var testCase = new BotTestCase()
            {
                Action = $"show output",
                ExpectedReply = "No runbook job id was specified. Try 'show <jobId> output'.",
                ErrorMessageHandler = (message, expected) => $"Show job output failed with message: '{message}'. The expected message is '{expected}'."
            };

            await TestRunner.RunTestCase(testCase);
        }

        [TestMethod]
        [TestCategory("Parallel")]
        public async Task ShowRunbookDescriptionShouldNotifyWhenTheSpecifiedRunbookDoesNotExist()
        {
            var runbook = "notfound";

            var testCase = new BotTestCase()
            {
                Action = $"show runbook {runbook} description",
                ExpectedReply = $"The '{runbook}' runbook was not found in any of your automation accounts.",
                ErrorMessageHandler = (message, expected) => $"Show runbook description failed with message: '{message}'. The expected message is '{expected}'."
            };

            await TestRunner.RunTestCase(testCase);
        }

        [TestMethod]
        [TestCategory("Parallel")]
        public async Task ShowRunbookDescriptionShouldNotifyWhenRunbookIsNotSpecified()
        {
            var testCase = new BotTestCase()
            {
                Action = $"show runbook description",
                ExpectedReply = "No runbook was specified. Please try again specifying a runbook name.",
                ErrorMessageHandler = (message, expected) => $"Show runbook description failed with message: '{message}'. The expected message is '{expected}'."
            };

            await TestRunner.RunTestCase(testCase);
        }

        [TestMethod]
        [TestCategory("Parallel")]
        public async Task ShouldShowRunbookDescription()
        {
            var runbook = this.TestContext.GetRunbookWithDescription();

            var testCase = new BotTestCase()
            {
                Action = $"show runbook {runbook} description",
                ExpectedReply = this.TestContext.GetRunbookDescription(),
                ErrorMessageHandler = (message, expected) => $"Show runbook description failed with message: '{message}'. The expected message is '{expected}'."
            };

            await TestRunner.RunTestCase(testCase);
        }

        [TestMethod]
        [TestCategory("Parallel")]
        public async Task ShowRunbookDescriptionShouldNotifyWhenTheSpecifiedRunbookDoesntHaveDescription()
        {
            var runbook = this.TestContext.GetRunbookWithoutDescription();

            var testCase = new BotTestCase()
            {
                Action = $"show runbook {runbook} description",
                ExpectedReply = "No description",
                ErrorMessageHandler = (message, expected) => $"Show runbook description failed with message: '{message}'. The expected message is '{expected}'."
            };

            await TestRunner.RunTestCase(testCase);
        }

        [TestMethod]
        [TestCategory("Parallel")]
        public async Task ShouldShowRunbookDescriptionsWhenSpecifiedRunbookExistsInMultipleAutomationAccounts()
        {
            var runbook = this.TestContext.GetRunbookInMultipleAutomationAccounts();

            var testCase = new BotTestCase()
            {
                Action = $"show runbook {runbook} description",
                ExpectedReply = $"I found the runbook '{runbook}' in multiple automation accounts. Showing the description of all of them:",
                ErrorMessageHandler = (message, expected) => $"Show runbook description failed with message: '{message}'. The expected message is '{expected}'."
            };

            await TestRunner.RunTestCase(testCase);
        }

        [TestMethod]
        [TestCategory("Parallel")]
        public async Task RunRunbookShouldNotifyWhenTheSpecifiedRunbookDoesNotExist()
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
        public async Task RunRunbookShouldNotifyWhenTheSpecifiedRunbookIsNotPublished()
        {
            var runbook = this.TestContext.GetRunbookNotPublished();

            var testCase = new BotTestCase()
            {
                Action = $"run runbook {runbook}",
                ExpectedReply = $"The '{runbook}' runbook that you are trying to run is not Published. Please go the Azure Portal and publish the runbook.",
                ErrorMessageHandler = (message, expected) => $"run runbook failed with message: '{message}'. The expected message is '{expected}'."
            };

            await TestRunner.RunTestCase(testCase);
        }

        [TestMethod]
        [TestCategory("Parallel")]
        public async Task RunRunbookShouldNotifyWhenTheSpecifiedAutomationAccountDoesNotExist()
        {
            var runbook = "test";
            var automationAccount = "notfound";

            var testCase = new BotTestCase()
            {
                Action = $"run runbook {runbook} from {automationAccount} automation account",
                ExpectedReply = $"The '{automationAccount}' automation account was not found in the current subscription",
                ErrorMessageHandler = (message, expected) => $"run runbook failed with message: '{message}'. The expected message is '{expected}'."
            };

            await TestRunner.RunTestCase(testCase);
        }

        [TestMethod]
        [TestCategory("Parallel")]
        public async Task RunRunbookShouldNotifyWhenTheSpecifiedRunbookDoesNotExistInTheSpecifiedAutomationAccount()
        {
            var runbook = "notfound";
            var automationAccount = this.TestContext.GetAutomationAcccount();

            var testCase = new BotTestCase()
            {
                Action = $"run runbook {runbook} from {automationAccount} automation account",
                ExpectedReply = $"The '{runbook}' runbook was not found in the '{automationAccount}' automation account.",
                ErrorMessageHandler = (message, expected) => $"run runbook failed with message: '{message}'. The expected message is '{expected}'."
            };

            await TestRunner.RunTestCase(testCase);
        }

        [TestMethod]
        [TestCategory("Parallel")]
        public async Task RunRunbookShouldNotifyWhenTheSpecifiedRunbookInTheSpecifiedAutomationAccountIsNotPublished()
        {
            var runbook = this.TestContext.GetRunbookNotPublished();
            var automationAccount = this.TestContext.GetAutomationAcccount();

            var testCase = new BotTestCase()
            {
                Action = $"run runbook {runbook} from {automationAccount} automation account",
                ExpectedReply = $"The '{runbook}' runbook that you are trying to run is not published (State:",
            ErrorMessageHandler = (message, expected) => $"run runbook failed with message: '{message}'. The expected message is '{expected}'."
            };

            await TestRunner.RunTestCase(testCase);
        }

        [TestMethod]
        public async Task ShouldShowStatusOfJobs()
        {
            var testCase = new BotTestCase()
            {
                Action = $"show status of my jobs",
                ExpectedReply = "|Id|Runbook|Start Time|End Time|Status|",
                ErrorMessageHandler = (message, expected) => $"Show status of my jobs failed with message: '{message}'. The expected message is '{expected}'."
            };

            await TestRunner.RunTestCase(testCase);
        }

        [TestMethod]
        public async Task ShouldRunRunbook()
        {
            Func<string, string, string> errorMessageHandler = (message, expected) => $"Run runbook failed with message: '{message}'. The expected message is '{expected}'.";

            var step1 = new BotTestCase()
            {
                Action = $"run runbook",
                ExpectedReply = $"Please select the automation account you want to use",
                ErrorMessageHandler = errorMessageHandler
            };

            var step2 = new BotTestCase()
            {
                Action = $"1",
                ExpectedReply = $"Please select the runbook you want to run",
                ErrorMessageHandler = errorMessageHandler
            };

            var step3 = new BotTestCase()
            {
                Action = $"2",
                ExpectedReply = $"Would you like to run runbook",
                ErrorMessageHandler = errorMessageHandler
            };

            var step4 = new BotTestCase()
            {
                Action = $"Yes",
                ExpectedReply = $"Created Job",
                ErrorMessageHandler = errorMessageHandler
            };

            var steps = new List<BotTestCase>() { step1, step2, step3, step4 };

            var completionStep1 = new BotTestCase()
            {
                ExpectedReply = $"is currently in 'Running' status",
                ErrorMessageHandler = errorMessageHandler
            };

            var completionStep2 = new BotTestCase()
            {
                ExpectedReply = $"is currently in 'Completed' status. Type **show job",
                ErrorMessageHandler = errorMessageHandler
            };

            var completionSteps = new List<BotTestCase>() { completionStep1, completionStep2 };

            await TestRunner.RunTestCases(steps, completionSteps, completionSteps.Count);
        }

        [TestMethod]
        public async Task ShouldRunSpecifiedRunbook()
        {
            Func<string, string, string> errorMessageHandler = (message, expected) => $"Run runbook failed with message: '{message}'. The expected message is '{expected}'.";

            var runbook = this.TestContext.GetRunbookWithDescription();

            var step1 = new BotTestCase()
            {
                Action = $"run runbook {runbook}",
                ExpectedReply = $"Would you like to run runbook '{runbook}' of automation acccount",
                ErrorMessageHandler = errorMessageHandler
            };

            var step2 = new BotTestCase()
            {
                Action = $"Yes",
                ExpectedReply = $"Created Job",
                ErrorMessageHandler = errorMessageHandler
            };

            var steps = new List<BotTestCase>() { step1, step2 };

            var completionStep1 = new BotTestCase()
            {
                ExpectedReply = $"is currently in 'Running' status",
                ErrorMessageHandler = errorMessageHandler
            };

            var completionStep2 = new BotTestCase()
            {
                ExpectedReply = $"is currently in 'Completed' status. Type **show job",
                ErrorMessageHandler = errorMessageHandler
            };

            var completionSteps = new List<BotTestCase>() { completionStep1, completionStep2 };

            await TestRunner.RunTestCases(steps, completionSteps, completionSteps.Count);
        }
    }
}
