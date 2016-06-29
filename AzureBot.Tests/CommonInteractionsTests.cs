namespace AzureBot.Tests
{
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Tests all common commands
    /// </summary>
    [TestClass]
    public class CommonInteractionsTests
    {
        [TestMethod]
        [TestCategory("Parallel")]
        public async Task ShouldShowHelp()
        {
            var testCase = new BotTestCase()
            {
                Action = "help",
                ExpectedReply = "Hello! You can use the Azure Bot to",
                ErrorMessageHandler = (message, expected) => $"Help failed with message: '{message}'. The expected message is '{expected}'."
            };

            await TestRunner.RunTestCase(testCase);
        }

        [TestMethod]
        [TestCategory("Parallel")]
        public async Task ShouldNotifyIfActionIsNotUnderstood()
        {
            var message = "I love AzureBot";
            var testCase = new BotTestCase()
            {
                Action = message,
                ExpectedReply = $"Sorry, I did not understand '{message}'. Type 'help' if you need assistance.",
                ErrorMessageHandler = (reply, expected) => $"Failed with message: '{reply}'. The expected message is '{expected}'."
            };

            await TestRunner.RunTestCase(testCase);
        }
    }
}
