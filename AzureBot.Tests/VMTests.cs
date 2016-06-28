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
        public TestContext TestContext { get; set; }

        [TestMethod]
        [TestCategory("Parallel")]
        public async Task ShoudListVms()
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
        public async Task StopVmShouldNotifyWhenNoVmsAreAvailableToStop()
        {
            Func<string, string, string> errorMessageHandler = (message, expected) => $"Stop vm failed with message: '{message}'. The expected message is '{expected}'.";

            var testCase = new BotTestCase()
            {
                Action = "stop vm",
                ExpectedReply = "No virtual machines that can be stopped were found in the current subscription.",
                ErrorMessageHandler = errorMessageHandler
            };

            await TestRunner.RunTestCase(testCase);
        }

        [TestMethod]
        public async Task StopAllVmsShouldNotifyWhenNoVmsAreAvailableToStop()
        {
            Func<string, string, string> errorMessageHandler = (message, expected) => $"Stop all vms failed with message: '{message}'. The expected message is '{expected}'.";

            var testCase = new BotTestCase()
            {
                Action = "stop all vms",
                ExpectedReply = "No virtual machines that can be stopped were found in the current subscription.",
                ErrorMessageHandler = errorMessageHandler
            };

            await TestRunner.RunTestCase(testCase);
        }

        [TestMethod]
        public async Task ShouldStartAllVms()
        {
            Func<string, string, string> errorMessageHandler = (message, expected) => $"Start all vms failed with message: '{message}'. The expected message is '{expected}'.";

            var step1 = new BotTestCase()
            {
                Action = "start all vms",
                ExpectedReply = "You are trying to start the following virtual machines",
                ErrorMessageHandler = errorMessageHandler
            };

            var step2 = new BotTestCase()
            {
                Action = "Yes",
                ExpectedReply = "Starting the following virtual machines",
                ErrorMessageHandler = errorMessageHandler
            };

            var completionTestCase = new BotTestCase()
            {
                ExpectedReply = "virtual machine was started successfully",
                ErrorMessageHandler = errorMessageHandler
            };

            var steps = new List<BotTestCase> { step1, step2 };

            await TestRunner.RunTestCases(steps, completionTestCase, 3);
        }

        [TestMethod]
        public async Task StartVmShouldNotifyWhenNoVmsAreAvailableToStart()
        {
            Func<string, string, string> errorMessageHandler = (message, expected) => $"Start vm failed with message: '{message}'. The expected message is '{expected}'.";

            var testCase = new BotTestCase()
            {
                Action = "start vm",
                ExpectedReply = "No virtual machines that can be started were found in the current subscription.",
                ErrorMessageHandler = errorMessageHandler
            };

            await TestRunner.RunTestCase(testCase);
        }

        [TestMethod]
        public async Task StartAllVmsShouldNotifyWhenNoVmsAreAvailableToStart()
        {
            Func<string, string, string> errorMessageHandler = (message, expected) => $"Start all vms failed with message: '{message}'. The expected message is '{expected}'.";

            var testCase = new BotTestCase()
            {
                Action = "start all vms",
                ExpectedReply = "No virtual machines that can be started were found in the current subscription.",
                ErrorMessageHandler = errorMessageHandler
            };

            await TestRunner.RunTestCase(testCase);
        }

        [TestMethod]
        public async Task StartVmShouldNotifyWhenTheSpecifiedVmIsAlreadyStarted()
        {
            Func<string, string, string> errorMessageHandler = (message, expected) => $"Start vm failed with message: '{message}'. The expected message is '{expected}'.";

            var virtualMachine = this.TestContext.GetVirtualMachine();

            var testCase = new BotTestCase()
            {
                Action = $"start vm {virtualMachine}",
                ExpectedReply = $"The '{virtualMachine}' virtual machine is already running.",
                ErrorMessageHandler = errorMessageHandler
            };

            await TestRunner.RunTestCase(testCase);
        }

        [TestMethod]
        [TestCategory("Parallel")]
        public async Task StartVmShouldNotifyWhenTheSpecifiedVmDoesNotExist()
        {
            Func<string, string, string> errorMessageHandler = (message, expected) => $"Start vm failed with message: '{message}'. The expected message is '{expected}'.";

            var virtualMachine = "notfound-start";

            var testCase = new BotTestCase()
            {
                Action = $"start vm {virtualMachine}",
                ExpectedReply = $"The '{virtualMachine}' virtual machine was not found in the current subscription.",
                ErrorMessageHandler = errorMessageHandler
            };

            await TestRunner.RunTestCase(testCase);
        }

        [TestMethod]
        public async Task ShouldShutdownVm()
        {
            Func<string, string, string> errorMessageHandler = (message, expected) => $"Shutdown vm failed with message: '{message}'. The expected message is '{expected}'.";

            var step1 = new BotTestCase()
            {
                Action = "shutdown vm",
                ExpectedReply = "Please select the virtual machine you want to shutdown",
                ErrorMessageHandler = errorMessageHandler
            };

            var step2 = new BotTestCase()
            {
                Action = "1",
                ExpectedReply = "Would you like to shutdown virtual machine",
                ErrorMessageHandler = errorMessageHandler
            };

            var step3 = new BotTestCase()
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

            var steps = new List<BotTestCase> { step1, step2, step3 };

            await TestRunner.RunTestCases(steps, completionTestCase);
        }

        [TestMethod]
        public async Task ShouldShutdownSpecifiedVm()
        {
            Func<string, string, string> errorMessageHandler = (message, expected) => $"Shutdown vm failed with message: '{message}'. The expected message is '{expected}'.";

            var virtualMachine = this.TestContext.GetVirtualMachine();

            var step1 = new BotTestCase()
            {
                Action = $"Shutdown vm {virtualMachine}",
                ExpectedReply = $"Would you like to shutdown virtual machine '{virtualMachine}",
                ErrorMessageHandler = errorMessageHandler
            };

            var step2 = new BotTestCase()
            {
                Action = "Yes",
                ExpectedReply = $"Shutting down the '{virtualMachine}' virtual machine...",
                ErrorMessageHandler = errorMessageHandler
            };

            var completionTestCase = new BotTestCase()
            {
                ExpectedReply = $"The '{virtualMachine}' virtual machine was shut down successfully.",
                ErrorMessageHandler = errorMessageHandler
            };

            var steps = new List<BotTestCase> { step1, step2 };

            await TestRunner.RunTestCases(steps, completionTestCase);
        }

        [TestMethod]
        public async Task ShutdownVmShouldNotifyWhenTheSpecifiedVmIsAlreadyShutdown()
        {
            Func<string, string, string> errorMessageHandler = (message, expected) => $"Shutdown vm failed with message: '{message}'. The expected message is '{expected}'.";

            var virtualMachine = this.TestContext.GetVirtualMachine();

            var testCase = new BotTestCase()
            {
                Action = $"shutdown vm {virtualMachine}",
                ExpectedReply = $"The '{virtualMachine}' virtual machine is already stopped.",
                ErrorMessageHandler = errorMessageHandler
            };

            await TestRunner.RunTestCase(testCase);
        }

        [TestMethod]
        [TestCategory("Parallel")]
        public async Task ShutdownVmShouldNotifyWhenTheSpecifiedVmDoesNotExist()
        {
            Func<string, string, string> errorMessageHandler = (message, expected) => $"Shutdown vm failed with message: '{message}'. The expected message is '{expected}'.";

            var virtualMachine = "notfound-shutdown";

            var testCase = new BotTestCase()
            {
                Action = $"shutdown vm {virtualMachine}",
                ExpectedReply = $"The '{virtualMachine}' virtual machine was not found in the current subscription.",
                ErrorMessageHandler = errorMessageHandler
            };

            await TestRunner.RunTestCase(testCase);
        }

        [TestMethod]
        public async Task ShouldStopVm()
        {
            Func<string, string, string> errorMessageHandler = (message, expected) => $"Stop vm failed with message: '{message}'. The expected message is '{expected}'.";

            var step1 = new BotTestCase()
            {
                Action = "stop vm",
                ExpectedReply = "Please select the virtual machine you want to stop",
                ErrorMessageHandler = errorMessageHandler
            };

            var step2 = new BotTestCase()
            {
                Action = "1",
                ExpectedReply = "Would you like to stop virtual machine",
                ErrorMessageHandler = errorMessageHandler
            };

            var step3 = new BotTestCase()
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

            var steps = new List<BotTestCase> { step1, step2, step3 };

            await TestRunner.RunTestCases(steps, completionTestCase);
        }

        [TestMethod]
        public async Task ShouldStopSpecifiedVm()
        {
            Func<string, string, string> errorMessageHandler = (message, expected) => $"Stop vm failed with message: '{message}'. The expected message is '{expected}'.";

            var virtualMachine = this.TestContext.GetVirtualMachine();

            var step1 = new BotTestCase()
            {
                Action = $"Stop vm {virtualMachine}",
                ExpectedReply = $"Would you like to stop virtual machine '{virtualMachine}",
                ErrorMessageHandler = errorMessageHandler
            };

            var step2 = new BotTestCase()
            {
                Action = "Yes",
                ExpectedReply = $"Stopping the '{virtualMachine}' virtual machine...",
                ErrorMessageHandler = errorMessageHandler
            };

            var completionTestCase = new BotTestCase()
            {
                ExpectedReply = $"The '{virtualMachine}' virtual machine was stopped successfully.",
                ErrorMessageHandler = errorMessageHandler
            };

            var steps = new List<BotTestCase> { step1, step2 };

            await TestRunner.RunTestCases(steps, completionTestCase);
        }

        [TestMethod]
        public async Task StopVmShouldNotifyWhenTheSpecifiedVmIsAlreadyDeallocated()
        {
            Func<string, string, string> errorMessageHandler = (message, expected) => $"Stop vm failed with message: '{message}'. The expected message is '{expected}'.";

            var virtualMachine = this.TestContext.GetVirtualMachine();

            var testCase = new BotTestCase()
            {
                Action = $"stop vm {virtualMachine}",
                ExpectedReply = $"The '{virtualMachine}' virtual machine is already deallocated.",
                ErrorMessageHandler = errorMessageHandler
            };

            await TestRunner.RunTestCase(testCase);
        }

        [TestMethod]
        [TestCategory("Parallel")]
        public async Task StopVmShouldNotifyWhenTheSpecifiedVmDoesNotExist()
        {
            Func<string, string, string> errorMessageHandler = (message, expected) => $"Stop vm failed with message: '{message}'. The expected message is '{expected}'.";

            var virtualMachine = "notfound-stop";

            var testCase = new BotTestCase()
            {
                Action = $"stop vm {virtualMachine}",
                ExpectedReply = $"The '{virtualMachine}' virtual machine was not found in the current subscription.",
                ErrorMessageHandler = errorMessageHandler
            };

            await TestRunner.RunTestCase(testCase);
        }

        [TestMethod]
        public async Task ShouldStartVm()
        {
            Func<string, string, string> errorMessageHandler = (message, expected) => $"Start vm failed with message: '{message}'. The expected message is '{expected}'.";

            var step1 = new BotTestCase()
            {
                Action = "start vm",
                ExpectedReply = "Please select the virtual machine you want to start",
                ErrorMessageHandler = errorMessageHandler
            };

            var step2 = new BotTestCase()
            {
                Action = "1",
                ExpectedReply = "Would you like to start virtual machine",
                ErrorMessageHandler = errorMessageHandler
            };

            var step3 = new BotTestCase()
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

            var steps = new List<BotTestCase> { step1, step2, step3 };

            await TestRunner.RunTestCases(steps, completionTestCase);
        }

        [TestMethod]
        public async Task ShouldStartSpecifiedVm()
        {
            Func<string, string, string> errorMessageHandler = (message, expected) => $"Start vm failed with message: '{message}'. The expected message is '{expected}'.";

            var virtualMachine = this.TestContext.GetVirtualMachine();

            var step1 = new BotTestCase()
            {
                Action = $"start vm {virtualMachine}",
                ExpectedReply = $"Would you like to start virtual machine '{virtualMachine}",
                ErrorMessageHandler = errorMessageHandler
            };

            var step2 = new BotTestCase()
            {
                Action = "Yes",
                ExpectedReply = $"Starting the '{virtualMachine}' virtual machine...",
                ErrorMessageHandler = errorMessageHandler
            };

            var completionTestCase = new BotTestCase()
            {
                ExpectedReply = $"The '{virtualMachine}' virtual machine was started successfully.",
                ErrorMessageHandler = errorMessageHandler
            };

            var steps = new List<BotTestCase> { step1, step2 };

            await TestRunner.RunTestCases(steps, completionTestCase);
        }

        [TestMethod]
        public async Task ShouldShutdownAllVmsFromSpecifiedResourceGroup()
        {
            Func<string, string, string> errorMessageHandler = (message, expected) => $"Shutdown all vms failed with message: '{message}'. The expected message is '{expected}'.";

            var resourceGroup = this.TestContext.GetResourceGroup();

            var step1 = new BotTestCase()
            {
                Action = $"shutdown all vms from {resourceGroup} resource group",
                ExpectedReply = "You are trying to shutdown the following virtual machines",
                ErrorMessageHandler = errorMessageHandler
            };

            var step2 = new BotTestCase()
            {
                Action = "Yes",
                ExpectedReply = "Shutting down the following virtual machines",
                ErrorMessageHandler = errorMessageHandler
            };

            var completionTestCase = new BotTestCase()
            {
                ExpectedReply = "virtual machine was shut down successfully",
                ErrorMessageHandler = errorMessageHandler
            };

            var steps = new List<BotTestCase> { step1, step2 };

            await TestRunner.RunTestCases(steps, completionTestCase, 2);
        }

        [TestMethod]
        public async Task ShutdownAllVmsFromSpecifiedResourceGroupShouldNotifyWhenNoVmsInTheResourceGroupAreAvailableToShutdown()
        {
            Func<string, string, string> errorMessageHandler = (message, expected) => $"Shutdown all vms failed with message: '{message}'. The expected message is '{expected}'.";

            var resourceGroup = this.TestContext.GetResourceGroup();

            var testCase = new BotTestCase()
            {
                Action = $"shutdown all vms from {resourceGroup} resource group",
                ExpectedReply = $"No virtual machines that can be shut down were found in the {resourceGroup} resource group of the current subscription.",
                ErrorMessageHandler = errorMessageHandler
            };

            await TestRunner.RunTestCase(testCase);
        }

        [TestMethod]
        [TestCategory("Parallel")]
        public async Task ShutdownAllVmsFromSpecifiedResourceGroupShouldNotifyWhenTheSpecifiedResourceGroupDoesNotExist()
        {
            Func<string, string, string> errorMessageHandler = (message, expected) => $"Shutdown all vms failed with message: '{message}'. The expected message is '{expected}'.";

            var resourceGroup = "NOTFOUND";

            var testCase = new BotTestCase()
            {
                Action = $"shutdown all vms from {resourceGroup} resource group",
                ExpectedReply = $"The {resourceGroup} resource group doesn't contain VMs or doesn't exist in the current subscription.",
                ErrorMessageHandler = errorMessageHandler
            };

            await TestRunner.RunTestCase(testCase);
        }

        [TestMethod]
        public async Task ShouldShutdownAllVms()
        {
            Func<string, string, string> errorMessageHandler = (message, expected) => $"Shutdown all vms failed with message: '{message}'. The expected message is '{expected}'.";

            var step1 = new BotTestCase()
            {
                Action = "shutdown all vms",
                ExpectedReply = "You are trying to shutdown the following virtual machines",
                ErrorMessageHandler = errorMessageHandler
            };

            var step2 = new BotTestCase()
            {
                Action = "Yes",
                ExpectedReply = "Shutting down the following virtual machines",
                ErrorMessageHandler = errorMessageHandler
            };

            var completionTestCase = new BotTestCase()
            {
                ExpectedReply = "virtual machine was shut down successfully",
                ErrorMessageHandler = errorMessageHandler
            };

            var steps = new List<BotTestCase> { step1, step2 };

            await TestRunner.RunTestCases(steps, completionTestCase, 1);
        }

        [TestMethod]
        public async Task ShutdownVmShouldNotifyWhenNoVmsAreAvailableToStart()
        {
            Func<string, string, string> errorMessageHandler = (message, expected) => $"Shutdown vm failed with message: '{message}'. The expected message is '{expected}'.";

            var testCase = new BotTestCase()
            {
                Action = "shutdown vm",
                ExpectedReply = "No virtual machines that can be shut down were found in the current subscription.",
                ErrorMessageHandler = errorMessageHandler
            };

            await TestRunner.RunTestCase(testCase);
        }

        [TestMethod]
        public async Task ShutdownAllVmsShouldNotifyWhenNoVmsAreAvailableToStart()
        {
            Func<string, string, string> errorMessageHandler = (message, expected) => $"Shutdown all vms failed with message: '{message}'. The expected message is '{expected}'.";

            var testCase = new BotTestCase()
            {
                Action = "shutdown all vms",
                ExpectedReply = "No virtual machines that can be shut down were found in the current subscription.",
                ErrorMessageHandler = errorMessageHandler
            };

            await TestRunner.RunTestCase(testCase);
        }

        [TestMethod]
        public async Task ShouldStopAllVmsFromSpecifiedResourceGroup()
        {
            Func<string, string, string> errorMessageHandler = (message, expected) => $"Stop all vms failed with message: '{message}'. The expected message is '{expected}'.";

            var resourceGroup = this.TestContext.GetResourceGroup();

            var step1 = new BotTestCase()
            {
                Action = $"stop all vms from {resourceGroup} resource group",
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
                ExpectedReply = "virtual machine was stopped successfully",
                ErrorMessageHandler = errorMessageHandler
            };

            var steps = new List<BotTestCase> { step1, step2 };

            await TestRunner.RunTestCases(steps, completionTestCase, 2);
        }

        [TestMethod]
        public async Task StopAllVmsFromSpecifiedResourceGroupShouldNotifyWhenNoVmsInTheResourceGroupAreAvailableToStop()
        {
            Func<string, string, string> errorMessageHandler = (message, expected) => $"Stop all vms failed with message: '{message}'. The expected message is '{expected}'.";

            var resourceGroup = this.TestContext.GetResourceGroup();

            var testCase = new BotTestCase()
            {
                Action = $"stop all vms from {resourceGroup} resource group",
                ExpectedReply = $"No virtual machines that can be stopped were found in the {resourceGroup} resource group of the current subscription.",
                ErrorMessageHandler = errorMessageHandler
            };

            await TestRunner.RunTestCase(testCase);
        }

        [TestMethod]
        [TestCategory("Parallel")]
        public async Task StopAllVmsFromSpecifiedResourceGroupShouldNotifyWhenTheSpecifiedResourceGroupDoesNotExist()
        {
            Func<string, string, string> errorMessageHandler = (message, expected) => $"Stop all vms failed with message: '{message}'. The expected message is '{expected}'.";

            var resourceGroup = "NOTFOUND";

            var testCase = new BotTestCase()
            {
                Action = $"stop all vms from {resourceGroup} resource group",
                ExpectedReply = $"The {resourceGroup} resource group doesn't contain VMs or doesn't exist in the current subscription.",
                ErrorMessageHandler = errorMessageHandler
            };

            await TestRunner.RunTestCase(testCase);
        }

        [TestMethod]
        public async Task ShouldStopAllVms()
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
                ExpectedReply = "virtual machine was stopped successfully",
                ErrorMessageHandler = errorMessageHandler
            };

            var steps = new List<BotTestCase> { step1, step2 };

            await TestRunner.RunTestCases(steps, completionTestCase, 1);
        }

        [TestMethod]
        public async Task ShouldStartAllVmsFromSpecifiedResourceGroup()
        {
            Func<string, string, string> errorMessageHandler = (message, expected) => $"Start all vms failed with message: '{message}'. The expected message is '{expected}'.";

            var resourceGroup = this.TestContext.GetResourceGroup();

            var step1 = new BotTestCase()
            {
                Action = $"start all vms from {resourceGroup} resource group",
                ExpectedReply = "You are trying to start the following virtual machines",
                ErrorMessageHandler = errorMessageHandler
            };

            var step2 = new BotTestCase()
            {
                Action = "Yes",
                ExpectedReply = "Starting the following virtual machines",
                ErrorMessageHandler = errorMessageHandler
            };

            var completionTestCase = new BotTestCase()
            {
                ExpectedReply = "virtual machine was started successfully",
                ErrorMessageHandler = errorMessageHandler
            };

            var steps = new List<BotTestCase> { step1, step2 };

            await TestRunner.RunTestCases(steps, completionTestCase, 2);
        }

        [TestMethod]
        public async Task StartAllVmsFromSpecifiedResourceGroupShouldNotifyWhenNoVmsInTheResourceGroupAreAvailableToStart()
        {
            Func<string, string, string> errorMessageHandler = (message, expected) => $"Start all vms failed with message: '{message}'. The expected message is '{expected}'.";

            var resourceGroup = this.TestContext.GetResourceGroup();

            var testCase = new BotTestCase()
            {
                Action = $"start all vms from {resourceGroup} resource group",
                ExpectedReply = $"No virtual machines that can be started were found in the {resourceGroup} resource group of the current subscription.",
                ErrorMessageHandler = errorMessageHandler
            };

            await TestRunner.RunTestCase(testCase);
        }

        [TestMethod]
        [TestCategory("Parallel")]
        public async Task StartAllVmsFromSpecifiedResourceGroupShouldNotifyWhenTheSpecifiedResourceGroupDoesNotExist()
        {
            Func<string, string, string> errorMessageHandler = (message, expected) => $"Start all vms failed with message: '{message}'. The expected message is '{expected}'.";

            var resourceGroup = "NOTFOUND";

            var testCase = new BotTestCase()
            {
                Action = $"start all vms from {resourceGroup} resource group",
                ExpectedReply = $"The {resourceGroup} resource group doesn't contain VMs or doesn't exist in the current subscription.",
                ErrorMessageHandler = errorMessageHandler
            };

            await TestRunner.RunTestCase(testCase);
        }
    }
}
