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
                ExpectedReply = "I can help you",
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
            };

            await TestRunner.RunTestCase(testCase);
        }
    }
}
