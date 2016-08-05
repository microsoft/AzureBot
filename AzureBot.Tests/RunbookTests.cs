namespace AzureBot.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Tests all Runbook commands
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
            };

            await TestRunner.RunTestCase(testCase);
        }

        [TestMethod]
        [TestCategory("Parallel")]
        public async Task ListRunbooksShouldNotifyWhenNoAutomationAccountsAreAvailable()
        {
            var step1 = GetStepToSwitchSubscription(this.TestContext.GetAlternativeSubscription());

            var step2 = new BotTestCase()
            {
                Action = "list my runbooks",
                ExpectedReply = "No runbooks listed since no automations accounts were found in the current subscription.",
            };

            var step3 = GetStepToSwitchSubscription(this.TestContext.GetSubscription());

            var steps = new List<BotTestCase>() { step1, step2, step3 };

            await TestRunner.RunTestCases(steps, new List<BotTestCase>());
        }

        [TestMethod]
        [TestCategory("Parallel")]
        public async Task ShoudListAutomationAccounts()
        {
            var testCase = new BotTestCase()
            {
                Action = "list automation accounts",
                ExpectedReply = "Available automations accounts are",
            };

            await TestRunner.RunTestCase(testCase);
        }

        [TestMethod]
        [TestCategory("Parallel")]
        public async Task ListAutomationAccountsShouldNotifyWhenNoAutomationAccountsAreAvailable()
        {
            var step1 = GetStepToSwitchSubscription(this.TestContext.GetAlternativeSubscription());

            var step2 = new BotTestCase()
            {
                Action = "list automation accounts",
                ExpectedReply = "No automations accounts were found in the current subscription.",
            };

            var step3 = GetStepToSwitchSubscription(this.TestContext.GetSubscription());

            var steps = new List<BotTestCase>() { step1, step2, step3 };

            await TestRunner.RunTestCases(steps, new List<BotTestCase>());
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
            };

            await TestRunner.RunTestCase(testCase);
        }

        [TestMethod]
        [TestCategory("Parallel")]
        public async Task RunRunbooksShouldNotifyWhenNoAutomationAccountsAreAvailable()
        {
            var step1 = GetStepToSwitchSubscription(this.TestContext.GetAlternativeSubscription());

            var step2 = new BotTestCase()
            {
                Action = "run runbook",
                ExpectedReply = "No automations accounts were found in the current subscription. Please create an Azure automation account or switch to a subscription which has an automation account in it.",
            };

            var step3 = GetStepToSwitchSubscription(this.TestContext.GetSubscription());

            var steps = new List<BotTestCase>() { step1, step2, step3 };

            await TestRunner.RunTestCases(steps, new List<BotTestCase>());
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
            };

            await TestRunner.RunTestCase(testCase);
        }

        [TestMethod]
        [TestCategory("Parallel")]
        public async Task ShowStatusOfJobsShouldNotifyIfNoJobsWereCreated()
        {
            var step1 = GetStepToSwitchSubscription(this.TestContext.GetAlternativeSubscription());

            var step2 = new BotTestCase()
            {
                Action = $"show status of my jobs",
                ExpectedReply = $"No Runbook Jobs were created in the current session. To create a new Runbook Job type: Start Runbook.",
            };

            var step3 = GetStepToSwitchSubscription(this.TestContext.GetSubscription());

            var steps = new List<BotTestCase>() { step1, step2, step3 };

            await TestRunner.RunTestCases(steps, new List<BotTestCase>());
        }

        [TestMethod]
        [TestCategory("Runbooks")]
        public async Task ShouldRunRunbook()
        {
            var step1 = new BotTestCase()
            {
                Action = $"run runbook",
                ExpectedReply = $"Please select the automation account you want to use",
            };

            var step2 = new BotTestCase()
            {
                Action = $"1",
                ExpectedReply = $"Please select the runbook you want to run",
            };

            var step3 = new BotTestCase()
            {
                Action = $"2",
                ExpectedReply = $"Would you like to run runbook",
            };

            var step4 = new BotTestCase()
            {
                Action = $"Yes",
                ExpectedReply = $"Created Job",
            };

            var steps = new List<BotTestCase>() { step1, step2, step3, step4 };

            var completionStep1 = new BotTestCase()
            {
                ExpectedReply = $"is currently in 'Running' status",
            };

            var completionStep2 = new BotTestCase()
            {
                ExpectedReply = $"is currently in 'Completed' status. Type *show job",
            };

            var completionSteps = new List<BotTestCase>() { completionStep1, completionStep2 };

            await TestRunner.RunTestCases(steps, completionSteps, completionSteps.Count);
        }

        [TestMethod]
        [TestCategory("Runbooks")]
        public async Task ShouldShowStatusOfJobs()
        {
            var testCase = new BotTestCase()
            {
                Action = $"show status of my jobs",
                ExpectedReply = "|job",
            };

            await TestRunner.RunTestCase(testCase);
        }

        [TestMethod]
        [TestCategory("Runbooks")]
        public async Task ShouldRunSpecifiedRunbook()
        {
            var runbook = this.TestContext.GetRunbookWithDescription();

            var step1 = new BotTestCase()
            {
                Action = $"run runbook {runbook}",
                ExpectedReply = $"Would you like to run runbook '{runbook}' of automation acccount",
            };

            var step2 = new BotTestCase()
            {
                Action = $"Yes",
                ExpectedReply = $"Created Job",
            };

            var steps = new List<BotTestCase>() { step1, step2 };

            var completionStep1 = new BotTestCase()
            {
                ExpectedReply = $"is currently in 'Running' status",
            };

            var completionStep2 = new BotTestCase()
            {
                ExpectedReply = $"Runbook '{runbook}' is currently in 'Completed' status. Type *show job",
            };

            var completionSteps = new List<BotTestCase>() { completionStep1, completionStep2 };

            await TestRunner.RunTestCases(steps, completionSteps, completionSteps.Count);
        }

        [TestMethod]
        [TestCategory("Runbooks")]
        public async Task ShouldRunSpecifiedRunbookFromSpecifiedAutomationAccount()
        {
            var runbook = this.TestContext.GetRunbookWithDescription();
            var automationAccount = this.TestContext.GetAutomationAcccount();

            var step1 = new BotTestCase()
            {
                Action = $"run runbook {runbook} from {automationAccount} automation account",
                ExpectedReply = $"Would you like to run runbook '{runbook}' of automation acccount '{automationAccount}'?",
            };

            var step2 = new BotTestCase()
            {
                Action = $"Yes",
                ExpectedReply = $"Created Job",
            };

            var steps = new List<BotTestCase>() { step1, step2 };

            var completionStep1 = new BotTestCase()
            {
                ExpectedReply = $"is currently in 'Running' status",
            };

            var completionStep2 = new BotTestCase()
            {
                ExpectedReply = $"Runbook '{runbook}' is currently in 'Completed' status. Type *show job",
            };

            var completionSteps = new List<BotTestCase>() { completionStep1, completionStep2 };

            await TestRunner.RunTestCases(steps, completionSteps, completionSteps.Count);
        }

        [TestMethod]
        [TestCategory("Runbooks")]
        public async Task RunRunbookShouldNotifyWhenSpecifiedRunbookFailsToComplete()
        {
            var runbook = this.TestContext.GetRunbookThatFails();

            var step1 = new BotTestCase()
            {
                Action = $"run runbook {runbook}",
                ExpectedReply = $"Would you like to run runbook '{runbook}' of automation acccount",
            };

            var step2 = new BotTestCase()
            {
                Action = $"Yes",
                ExpectedReply = $"Created Job",
            };

            var steps = new List<BotTestCase>() { step1, step2 };

            var completionStep1 = new BotTestCase()
            {
                ExpectedReply = $"is currently in 'Running' status",
            };

            var completionStep2 = new BotTestCase()
            {
                ExpectedReply = $"did not complete with status 'Failed'. Please go to the Azure Portal for more detailed information on why.",
            };

            var completionSteps = new List<BotTestCase>() { completionStep1, completionStep2 };

            await TestRunner.RunTestCases(steps, completionSteps, completionSteps.Count, false);
        }

        [TestMethod]
        [TestCategory("Runbooks")]
        public async Task ShouldRunRunbookThatNeedsParameter()
        {
            var runbook = this.TestContext.GetRunbookWithParameters();
            var automationAccount = this.TestContext.GetAutomationAcccount();

            var step1 = new BotTestCase()
            {
                Action = $"run runbook {runbook} from {automationAccount} automation account",
                ExpectedReply = $"Would you like to run runbook '{runbook}' of automation acccount '{automationAccount}",
            };

            var step2 = new BotTestCase()
            {
                Action = $"Yes",
                ExpectedReply = $"If you're unsure what to input, type *quit* followed by *show runbook {runbook} description* to get more details.",
            };

            var step3 = new BotTestCase()
            {
                Action = $"UnitTests",
                ExpectedReply = $"Please enter the value for",
            };

            var step4 = new BotTestCase()
            {
                Action = $"none",
                ExpectedReply = $"Created Job",
            };

            var steps = new List<BotTestCase>() { step1, step2, step3, step4 };

            var completionStep1 = new BotTestCase()
            {
                ExpectedReply = $"is currently in 'Running' status",
            };

            var completionStep2 = new BotTestCase()
            {
                ExpectedReply = $"Runbook '{runbook}' is currently in 'Completed' status. Type *show job",
            };

            var completionSteps = new List<BotTestCase>() { completionStep1, completionStep2 };

            await TestRunner.RunTestCases(steps, completionSteps, completionSteps.Count);
        }

        [TestMethod]
        [TestCategory("Runbooks")]
        public async Task ShouldShowJobOutput()
        {
            var runbook = this.TestContext.GetRunbookWithDescription();

            var step1 = new BotTestCase()
            {
                Action = $"run runbook {runbook}",
                ExpectedReply = $"Would you like to run runbook '{runbook}' of automation acccount",
            };

            var step2 = new BotTestCase()
            {
                Action = $"Yes",
                ExpectedReply = $"Created Job",
            };

            var steps = new List<BotTestCase>() { step1, step2 };

            var completionStep1 = new BotTestCase()
            {
                ExpectedReply = $"is currently in 'Running' status",
            };

            string jobId = null;

            var completionStep2 = new BotTestCase()
            {
                ExpectedReply = $"Runbook '{runbook}' is currently in 'Completed' status. Type *show job",
                Verified = (reply) =>
                {
                    var jobIndex = reply.LastIndexOf("job");
                    jobId = reply.Substring(jobIndex, reply.Substring(jobIndex).IndexOf(" "));
                }
            };

            var completionSteps = new List<BotTestCase>() { completionStep1, completionStep2 };

            await TestRunner.RunTestCases(steps, completionSteps, completionSteps.Count);

            var jobOutput = this.TestContext.GetJobOutput();

            var showOutputTestCase = new BotTestCase()
            {
                Action = $"show {jobId} output",
                ExpectedReply = jobOutput,
            };

            await TestRunner.RunTestCase(showOutputTestCase);
        }

        [TestMethod]
        [TestCategory("Runbooks")]
        public async Task ShowJobOutputShouldNotifyWhenSpecifiedJobDoesntHaveOutput()
        {
            var runbook = this.TestContext.GetRunbookThatFails();

            var step1 = new BotTestCase()
            {
                Action = $"run runbook {runbook}",
                ExpectedReply = $"Would you like to run runbook '{runbook}' of automation acccount",
            };

            var step2 = new BotTestCase()
            {
                Action = $"Yes",
                ExpectedReply = $"Created Job",
            };

            var steps = new List<BotTestCase>() { step1, step2 };

            var completionStep1 = new BotTestCase()
            {
                ExpectedReply = $"is currently in 'Running' status",
            };

            var completionStep2 = new BotTestCase()
            {
                ExpectedReply = $"did not complete with status 'Failed'. Please go to the Azure Portal for more detailed information on why.",
            };

            var completionSteps = new List<BotTestCase>() { completionStep1, completionStep2 };

            await TestRunner.RunTestCases(steps, completionSteps, completionSteps.Count, false);

            string lastJobId = null;

            var statusOfJobsTestCase = new BotTestCase()
            {
                Action = $"show status of jobs",
                ExpectedReply = "|job",
                Verified = (reply) =>
                {
                    var lastJob = reply.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries).Last();

                    lastJobId = lastJob.Substring(1, lastJob.Substring(1).IndexOf("|"));
                }
            };

            await TestRunner.RunTestCase(statusOfJobsTestCase);

            var showOutputTestCase = new BotTestCase()
            {
                Action = $"show {lastJobId} output",
                ExpectedReply = $"No output for job '{lastJobId}'",
            };

            await TestRunner.RunTestCase(showOutputTestCase);
        }

        private static BotTestCase GetStepToSwitchSubscription(string subscription)
        {
            return new BotTestCase()
            {
                Action = $"switch subscription {subscription}",
                ExpectedReply = $"Setting {subscription} as the current subscription. What would you like to do next?",
            };
        }
    }
}
