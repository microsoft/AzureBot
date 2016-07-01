namespace AzureBot.Tests
{
    using System;
    using System.Collections.Generic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class General
    {
        private static BotHelper botHelper;
        private static TestContext testContext;

        internal static BotHelper BotHelper
        {
            get { return botHelper; }
        }

        // Will run once before all of the tests in the project. We start assuming the user is already logged in to Azure,
        // which should  be done separately via the AzureBot.ConsoleConversation or some other means. 
        [AssemblyInitialize]
        public static void SetUp(TestContext context)
        {
            testContext = context;
            string directLineToken = context.Properties["DirectLineToken"].ToString();
            string appId = context.Properties["AppId"].ToString();
            string fromUser = context.Properties["FromUser"].ToString();

            botHelper = new BotHelper(directLineToken, appId, fromUser);

            var subscription = context.GetSubscription();

            var testCase = new BotTestCase()
            {
                Action = $"select subscription {subscription}",
                ExpectedReply = $"Setting {subscription} as the current subscription. What would you like to do next?",
            };

            TestRunner.RunTestCase(testCase).Wait();
        }

        // Will run after all the tests have finished
        [AssemblyCleanup]
        public static void CleanUp()
        {
            if (testContext.DeallocateResourcesOnCleanup())
            {
                var step1 = new BotTestCase()
                {
                    Action = "stop all vms",
                    ExpectedReply = "You are trying to stop the following virtual machines",
                };

                var step2 = new BotTestCase()
                {
                    Action = "Yes",
                    ExpectedReply = "Stopping the following virtual machines",
                };

                var completionTestCase = new BotTestCase()
                {
                    ExpectedReply = $"virtual machine was stopped successfully.",
                };

                var steps = new List<BotTestCase> { step1, step2 };

                TestRunner.RunTestCases(steps, completionTestCase, 2).Wait();
            }

            if (botHelper != null)
            {
                botHelper.Dispose();
            }
        }
    }
}
