namespace AzureBot.Tests
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Tests all Subscription commands
    /// </summary>
    [TestClass]
    public class SubscriptionTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        [TestCategory("Parallel")]
        public async Task ShoudListSubscriptions()
        {
            var testCase = new BotTestCase()
            {
                Action = "list subscriptions",
                ExpectedReply = "Your subscriptions are",
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
            };

            await TestRunner.RunTestCase(testCase);
        }

        [TestMethod]
        [TestCategory("Parallel")]
        public async Task ShouldSwitchSubscription()
        {
            var subscription = this.TestContext.GetAlternativeSubscription();

            var step1 = new BotTestCase()
            {
                Action = "switch subscription",
                ExpectedReply = $"Please select the subscription you want to work with: ",
            };

            var step2 = new BotTestCase()
            {
                Action = subscription,
                ExpectedReply = $"Setting {subscription} as the current subscription. What would you like to do next?",
            };

            var steps = new List<BotTestCase> { step1, step2 };

            await TestRunner.RunTestCases(steps, new List<BotTestCase>());

            await this.ShouldSwitchToSpecifiedSubscription();
        }

        [TestMethod]
        [TestCategory("Parallel")]
        public async Task ShouldSwitchToSpecifiedSubscription()
        {
            var subscription = this.TestContext.GetSubscription();

            var testCase = new BotTestCase()
            {
                Action = $"switch subscription {subscription}",
                ExpectedReply = $"Setting {subscription} as the current subscription. What would you like to do next?",
            };

            await TestRunner.RunTestCase(testCase);
        }
    }
}
