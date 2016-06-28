namespace AzureBot.Tests
{
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Tests all VM commands
    /// </summary>
    [TestClass]
    public class SubscriptionTests
    {
        [TestMethod]
        [TestCategory("Parallel")]
        public async Task ShoudListSubscriptions()
        {
            var testCase = new BotTestCase()
            {
                Action = "list subscriptions",
                ExpectedReply = "Your subscriptions are",
                ErrorMessageHandler = (message, expected) => $"List subscriptions failed with message: '{message}'. The expected message is '{expected}'."
            };

            await TestRunner.RunTestCase(testCase);
        }

        [TestMethod]
        [TestCategory("Parallel")]
        public async Task ShouldShowCurrentSubscription()
        {
            var testCase = new BotTestCase()
            {
                Action = "What's my current subscription?",
                ExpectedReply = "Your current subscription is",
                ErrorMessageHandler = (message, expected) => $"Current subscription failed with message: '{message}'. The expected message is '{expected}'."
            };

            await TestRunner.RunTestCase(testCase);
        }

        [TestMethod]
        [TestCategory("Parallel")]
        public async Task SwitchSubscriptionShouldNotifyWhenTheSpecifiedSubscriptionDoesNotExist()
        {
            var subscription = "notfound";

            var testCase = new BotTestCase()
            {
                Action = $"switch subscription {subscription}",
                ExpectedReply = $"The '{subscription}' subscription was not found.",
                ErrorMessageHandler = (message, expected) => $"Switch subscription failed with message: '{message}'. The expected message is '{expected}'."
            };

            await TestRunner.RunTestCase(testCase);
        }
    }
}
