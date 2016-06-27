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
    public class VMTests
    {
        [TestMethod]
        [TestCategory("Always")]
        public async Task ShoudListVMs()
        {
            var testCase = new BotTestCase()
            {
                Action = "list vms",
                ExpectedReply = "Available VMs are",
                ErrorMessageHandler = (message, expected) => $"List vms failed with message: '{message}'. The expected message is '{expected}'."
            };

            await TestRunner.RunTestCase(testCase);
        }

        [TestMethod]
        public async Task ShouldStartVM()
        {
            Func<string, string, string> errorMessageHandler = (message, expected) => $"Start vm failed with message: '{message}'. The expected message is '{expected}'.";

            var testCase1 = new BotTestCase()
            {
                Action = "start vm",
                ExpectedReply = "Please select the virtual machine you want to start",
                ErrorMessageHandler = errorMessageHandler
            };

            var testCase2 = new BotTestCase()
            {
                Action = "1",
                ExpectedReply = "Would you like to start virtual machine",
                ErrorMessageHandler = errorMessageHandler
            };

            var testCase3 = new BotTestCase()
            {
                Action = "Yes",
                ExpectedReply = "Starting the",
                ErrorMessageHandler = errorMessageHandler
            };

            var completionTestCase = new BotTestCase()
            {
                ExpectedReply = "virtual machine was started successfully",
                ErrorMessageHandler = errorMessageHandler
            };

            var testCases = new List<BotTestCase> { testCase1, testCase2, testCase3 };

            await TestRunner.RunTestCases(testCases, completionTestCase);
        }

        [TestMethod]
        public async Task ShouldShutdownVM()
        {
            Func<string, string, string> errorMessageHandler = (message, expected) => $"Shutdown vm failed with message: '{message}'. The expected message is '{expected}'.";

            var testCase1 = new BotTestCase()
            {
                Action = "shutdown vm",
                ExpectedReply = "Please select the virtual machine you want to shutdown",
                ErrorMessageHandler = errorMessageHandler
            };

            var testCase2 = new BotTestCase()
            {
                Action = "1",
                ExpectedReply = "Would you like to shutdown virtual machine",
                ErrorMessageHandler = errorMessageHandler
            };

            var testCase3 = new BotTestCase()
            {
                Action = "Yes",
                ExpectedReply = "Shutting down the",
                ErrorMessageHandler = errorMessageHandler
            };

            var completionTestCase = new BotTestCase()
            {
                ExpectedReply = "virtual machine was shut down successfully",
                ErrorMessageHandler = errorMessageHandler
            };

            var testCases = new List<BotTestCase> { testCase1, testCase2, testCase3 };

            await TestRunner.RunTestCases(testCases, completionTestCase);
        }

        [TestMethod]
        public async Task ShouldStopVM()
        {
            Func<string, string, string> errorMessageHandler = (message, expected) => $"Stop vm failed with message: '{message}'. The expected message is '{expected}'.";

            var testCase1 = new BotTestCase()
            {
                Action = "stop vm",
                ExpectedReply = "Please select the virtual machine you want to stop",
                ErrorMessageHandler = errorMessageHandler
            };

            var testCase2 = new BotTestCase()
            {
                Action = "1",
                ExpectedReply = "Would you like to stop virtual machine",
                ErrorMessageHandler = errorMessageHandler
            };

            var testCase3 = new BotTestCase()
            {
                Action = "Yes",
                ExpectedReply = "Stopping the",
                ErrorMessageHandler = errorMessageHandler
            };

            var completionTestCase = new BotTestCase()
            {
                ExpectedReply = "virtual machine was stopped successfully",
                ErrorMessageHandler = errorMessageHandler
            };

            var testCases = new List<BotTestCase> { testCase1, testCase2, testCase3 };

            await TestRunner.RunTestCases(testCases, completionTestCase);
        }
    }
}
