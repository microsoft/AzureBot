namespace AzureBot.Tests
{
    using System;
    using System.Collections.Generic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class General
    {
        private static BotHelper botHelper;

        internal static BotHelper BotHelper
        {
            get { return botHelper; }
        }

        // Will run once before all of the tests in the project. We start assuming the user is already logged in to Azure,
        // which should  be done separately via the AzureBot.ConsoleConversation or some other means. 
        [AssemblyInitialize]
        public static void SetUp(TestContext context)
        {
            string directLineToken = context.Properties["DirectLineToken"].ToString();
            string appId = context.Properties["AppId"].ToString();
            string fromUser = context.Properties["FromUser"].ToString();

            botHelper = new BotHelper(directLineToken, appId, fromUser);

            Func<string, string, string> errorMessageHandler = (message, expected) => $"Setup failed with message: '{message}'. The expected message is '{expected}'.";

            var subscription = context.GetSubscription();

            var step1 = new BotTestCase()
            {
                Action = "select subscription",
                ExpectedReply = "Please select the subscription",
                ErrorMessageHandler = errorMessageHandler
            };

            var step2 = new BotTestCase()
            {
                Action = subscription,
                ExpectedReply = $"Setting {subscription}",
                ErrorMessageHandler = errorMessageHandler
            };

            var steps = new List<BotTestCase> { step1, step2 };

            TestRunner.RunTestCases(steps).Wait();
        }

        // Will run after all the tests have finished
        [AssemblyCleanup]
        public static void CleanUp()
        {
            Func<string, string, string> errorMessageHandler = (message, expected) => $"Stop all vms failed with message: '{message}'. The expected message is '{expected}'.";

            var step1 = new BotTestCase()
            {
                Action = "stop all vms",
                ExpectedReply = "You are trying to stop the following virtual machines",
                ErrorMessageHandler = errorMessageHandler
            };

            var step2 = new BotTestCase()
            {
                Action = "Yes",
                ExpectedReply = "Stopping the following virtual machines",
                ErrorMessageHandler = errorMessageHandler
            };

            var completionTestCase = new BotTestCase()
            {
                ExpectedReply = $"virtual machine was stopped successfully.",
                ErrorMessageHandler = errorMessageHandler
            };

            var steps = new List<BotTestCase> { step1, step2 };

            TestRunner.RunTestCases(steps, completionTestCase, 2).Wait();

            if (botHelper != null)
            {
                botHelper.Dispose();
            }
        }
    }
}
