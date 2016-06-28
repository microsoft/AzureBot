namespace AzureBot.Tests
{
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
    }
}
