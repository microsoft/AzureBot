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
            };

            await TestRunner.RunTestCase(testCase);
        }

        [TestMethod]
        [TestCategory("Parallel")]
        public async Task ListVmsShouldNotifyWhenNoVmsAreAvailable()
        {
            var step1 = GetStepToSwitchSubscription(this.TestContext.GetAlternativeSubscription());

            var step2 = new BotTestCase()
            {
                Action = "list vms",
                ExpectedReply = "No virtual machines were found in the current subscription.",
            };

            var step3 = GetStepToSwitchSubscription(this.TestContext.GetSubscription());

            var steps = new List<BotTestCase>() { step1, step2, step3 };

            await TestRunner.RunTestCases(steps, new List<BotTestCase>());
        }

        [TestMethod]
        [TestCategory("VMs")]
        public async Task StopVmShouldNotifyWhenNoVmsAreAvailableToStop()
        {
            var testCase = new BotTestCase()
            {
                Action = "stop vm",
                ExpectedReply = "No virtual machines that can be stopped were found in the current subscription.",
            };

            await TestRunner.RunTestCase(testCase);
        }

        [TestMethod]
        [TestCategory("VMs")]
        public async Task StopAllVmsShouldNotifyWhenNoVmsAreAvailableToStop()
        {
            var testCase = new BotTestCase()
            {
                Action = "stop all vms",
                ExpectedReply = "No virtual machines that can be stopped were found in the current subscription.",
            };

            await TestRunner.RunTestCase(testCase);
        }

        [TestMethod]
        [TestCategory("VMs")]
        public async Task ShouldStartAllVms()
        {
            var step1 = new BotTestCase()
            {
                Action = "start all vms",
                ExpectedReply = "You are trying to start the following virtual machines",
            };

            var step2 = new BotTestCase()
            {
                Action = "Yes",
                ExpectedReply = "Starting the following virtual machines",
            };

            var completionTestCase = new BotTestCase()
            {
                ExpectedReply = "virtual machine was started successfully",
            };

            var steps = new List<BotTestCase> { step1, step2 };

            await TestRunner.RunTestCases(steps, completionTestCase, 3);
        }

        [TestMethod]
        [TestCategory("VMs")]
        public async Task StartVmShouldNotifyWhenNoVmsAreAvailableToStart()
        {
            var testCase = new BotTestCase()
            {
                Action = "start vm",
                ExpectedReply = "No virtual machines that can be started were found in the current subscription.",
            };

            await TestRunner.RunTestCase(testCase);
        }

        [TestMethod]
        [TestCategory("VMs")]
        public async Task StartAllVmsShouldNotifyWhenNoVmsAreAvailableToStart()
        {
            var testCase = new BotTestCase()
            {
                Action = "start all vms",
                ExpectedReply = "No virtual machines that can be started were found in the current subscription.",
            };

            await TestRunner.RunTestCase(testCase);
        }

        [TestMethod]
        [TestCategory("VMs")]
        public async Task StartVmShouldNotifyWhenTheSpecifiedVmIsAlreadyStarted()
        {
            var virtualMachine = this.TestContext.GetVirtualMachine();

            var testCase = new BotTestCase()
            {
                Action = $"start vm {virtualMachine}",
                ExpectedReply = $"The '{virtualMachine}' virtual machine is already running.",
            };

            await TestRunner.RunTestCase(testCase);
        }

        [TestMethod]
        [TestCategory("Parallel")]
        public async Task StartVmShouldNotifyWhenTheSpecifiedVmDoesNotExist()
        {
            var virtualMachine = "notfound-start";

            var testCase = new BotTestCase()
            {
                Action = $"start vm {virtualMachine}",
                ExpectedReply = $"The '{virtualMachine}' virtual machine was not found in the current subscription.",
            };

            await TestRunner.RunTestCase(testCase);
        }

        [TestMethod]
        [TestCategory("VMs")]
        public async Task ShouldShutdownVm()
        {
            var step1 = new BotTestCase()
            {
                Action = "shutdown vm",
                ExpectedReply = "Please select the virtual machine you want to shutdown",
            };

            var step2 = new BotTestCase()
            {
                Action = "1",
                ExpectedReply = "Would you like to shutdown virtual machine",
            };

            var step3 = new BotTestCase()
            {
                Action = "Yes",
                ExpectedReply = "Shutting down the",
            };

            var completionTestCase = new BotTestCase()
            {
                ExpectedReply = "virtual machine was shut down successfully",
            };

            var steps = new List<BotTestCase> { step1, step2, step3 };

            await TestRunner.RunTestCases(steps, completionTestCase);
        }

        [TestMethod]
        [TestCategory("VMs")]
        public async Task ShouldShutdownSpecifiedVm()
        {
            var virtualMachine = this.TestContext.GetVirtualMachine();

            var step1 = new BotTestCase()
            {
                Action = $"Shutdown vm {virtualMachine}",
                ExpectedReply = $"Would you like to shutdown virtual machine '{virtualMachine}",
            };

            var step2 = new BotTestCase()
            {
                Action = "Yes",
                ExpectedReply = $"Shutting down the '{virtualMachine}' virtual machine...",
            };

            var completionTestCase = new BotTestCase()
            {
                ExpectedReply = $"The '{virtualMachine}' virtual machine was shut down successfully.",
            };

            var steps = new List<BotTestCase> { step1, step2 };

            await TestRunner.RunTestCases(steps, completionTestCase);
        }

        [TestMethod]
        [TestCategory("VMs")]
        public async Task ShutdownVmShouldNotifyWhenTheSpecifiedVmIsAlreadyShutdown()
        {
            var virtualMachine = this.TestContext.GetVirtualMachine();

            var testCase = new BotTestCase()
            {
                Action = $"shutdown vm {virtualMachine}",
                ExpectedReply = $"The '{virtualMachine}' virtual machine is already stopped.",
            };

            await TestRunner.RunTestCase(testCase);
        }

        [TestMethod]
        [TestCategory("Parallel")]
        public async Task ShutdownVmShouldNotifyWhenTheSpecifiedVmDoesNotExist()
        {
            var virtualMachine = "notfound-shutdown";

            var testCase = new BotTestCase()
            {
                Action = $"shutdown vm {virtualMachine}",
                ExpectedReply = $"The '{virtualMachine}' virtual machine was not found in the current subscription.",
            };

            await TestRunner.RunTestCase(testCase);
        }

        [TestMethod]
        [TestCategory("VMs")]
        public async Task ShouldStopVm()
        {
            var step1 = new BotTestCase()
            {
                Action = "stop vm",
                ExpectedReply = "Please select the virtual machine you want to stop",
            };

            var step2 = new BotTestCase()
            {
                Action = "1",
                ExpectedReply = "Would you like to stop virtual machine",
            };

            var step3 = new BotTestCase()
            {
                Action = "Yes",
                ExpectedReply = "Stopping the",
            };

            var completionTestCase = new BotTestCase()
            {
                ExpectedReply = "virtual machine was stopped successfully",
            };

            var steps = new List<BotTestCase> { step1, step2, step3 };

            await TestRunner.RunTestCases(steps, completionTestCase);
        }

        [TestMethod]
        [TestCategory("VMs")]
        public async Task ShouldStopSpecifiedVm()
        {
            var virtualMachine = this.TestContext.GetVirtualMachine();

            var step1 = new BotTestCase()
            {
                Action = $"Stop vm {virtualMachine}",
                ExpectedReply = $"Would you like to stop virtual machine '{virtualMachine}",
            };

            var step2 = new BotTestCase()
            {
                Action = "Yes",
                ExpectedReply = $"Stopping the '{virtualMachine}' virtual machine...",
            };

            var completionTestCase = new BotTestCase()
            {
                ExpectedReply = $"The '{virtualMachine}' virtual machine was stopped successfully.",
            };

            var steps = new List<BotTestCase> { step1, step2 };

            await TestRunner.RunTestCases(steps, completionTestCase);
        }

        [TestMethod]
        [TestCategory("VMs")]
        public async Task StopVmShouldNotifyWhenTheSpecifiedVmIsAlreadyDeallocated()
        {
            var virtualMachine = this.TestContext.GetVirtualMachine();

            var testCase = new BotTestCase()
            {
                Action = $"stop vm {virtualMachine}",
                ExpectedReply = $"The '{virtualMachine}' virtual machine is already deallocated.",
            };

            await TestRunner.RunTestCase(testCase);
        }

        [TestMethod]
        [TestCategory("Parallel")]
        public async Task StopVmShouldNotifyWhenTheSpecifiedVmDoesNotExist()
        {
            var virtualMachine = "notfound-stop";

            var testCase = new BotTestCase()
            {
                Action = $"stop vm {virtualMachine}",
                ExpectedReply = $"The '{virtualMachine}' virtual machine was not found in the current subscription.",
            };

            await TestRunner.RunTestCase(testCase);
        }

        [TestMethod]
        [TestCategory("VMs")]
        public async Task ShouldStartVm()
        {
            var step1 = new BotTestCase()
            {
                Action = "start vm",
                ExpectedReply = "Please select the virtual machine you want to start",
            };

            var step2 = new BotTestCase()
            {
                Action = "1",
                ExpectedReply = "Would you like to start virtual machine",
            };

            var step3 = new BotTestCase()
            {
                Action = "Yes",
                ExpectedReply = "Starting the",
            };

            var completionTestCase = new BotTestCase()
            {
                ExpectedReply = "virtual machine was started successfully",
            };

            var steps = new List<BotTestCase> { step1, step2, step3 };

            await TestRunner.RunTestCases(steps, completionTestCase);
        }

        [TestMethod]
        [TestCategory("VMs")]
        public async Task ShouldStartSpecifiedVm()
        {
            var virtualMachine = this.TestContext.GetVirtualMachine();

            var step1 = new BotTestCase()
            {
                Action = $"start vm {virtualMachine}",
                ExpectedReply = $"Would you like to start virtual machine '{virtualMachine}",
            };

            var step2 = new BotTestCase()
            {
                Action = "Yes",
                ExpectedReply = $"Starting the '{virtualMachine}' virtual machine...",
            };

            var completionTestCase = new BotTestCase()
            {
                ExpectedReply = $"The '{virtualMachine}' virtual machine was started successfully.",
            };

            var steps = new List<BotTestCase> { step1, step2 };

            await TestRunner.RunTestCases(steps, completionTestCase);
        }

        [TestMethod]
        [TestCategory("VMs")]
        public async Task ShouldShutdownAllVmsFromSpecifiedResourceGroup()
        {
            var resourceGroup = this.TestContext.GetResourceGroup();

            var step1 = new BotTestCase()
            {
                Action = $"shutdown all vms from {resourceGroup} resource group",
                ExpectedReply = "You are trying to shutdown the following virtual machines",
            };

            var step2 = new BotTestCase()
            {
                Action = "Yes",
                ExpectedReply = "Shutting down the following virtual machines",
            };

            var completionTestCase = new BotTestCase()
            {
                ExpectedReply = "virtual machine was shut down successfully",
            };

            var steps = new List<BotTestCase> { step1, step2 };

            await TestRunner.RunTestCases(steps, completionTestCase, 2);
        }

        [TestMethod]
        [TestCategory("VMs")]
        public async Task ShutdownAllVmsFromSpecifiedResourceGroupShouldNotifyWhenNoVmsInTheResourceGroupAreAvailableToShutdown()
        {
            var resourceGroup = this.TestContext.GetResourceGroup();

            var testCase = new BotTestCase()
            {
                Action = $"shutdown all vms from {resourceGroup} resource group",
                ExpectedReply = $"No virtual machines that can be shut down were found in the {resourceGroup} resource group of the current subscription.",
            };

            await TestRunner.RunTestCase(testCase);
        }

        [TestMethod]
        [TestCategory("Parallel")]
        public async Task ShutdownAllVmsFromSpecifiedResourceGroupShouldNotifyWhenTheSpecifiedResourceGroupDoesNotExist()
        {
            var resourceGroup = "NOTFOUND";

            var testCase = new BotTestCase()
            {
                Action = $"shutdown all vms from {resourceGroup} resource group",
                ExpectedReply = $"The {resourceGroup} resource group doesn't contain VMs or doesn't exist in the current subscription.",
            };

            await TestRunner.RunTestCase(testCase);
        }

        [TestMethod]
        [TestCategory("VMs")]
        public async Task ShouldShutdownAllVms()
        {
            var step1 = new BotTestCase()
            {
                Action = "shutdown all vms",
                ExpectedReply = "You are trying to shutdown the following virtual machines",
            };

            var step2 = new BotTestCase()
            {
                Action = "Yes",
                ExpectedReply = "Shutting down the following virtual machines",
            };

            var completionTestCase = new BotTestCase()
            {
                ExpectedReply = "virtual machine was shut down successfully",
            };

            var steps = new List<BotTestCase> { step1, step2 };

            await TestRunner.RunTestCases(steps, completionTestCase, 1);
        }

        [TestMethod]
        [TestCategory("VMs")]
        public async Task ShutdownVmShouldNotifyWhenNoVmsAreAvailableToStart()
        {
            var testCase = new BotTestCase()
            {
                Action = "shutdown vm",
                ExpectedReply = "No virtual machines that can be shut down were found in the current subscription.",
            };

            await TestRunner.RunTestCase(testCase);
        }

        [TestMethod]
        [TestCategory("VMs")]
        public async Task ShutdownAllVmsShouldNotifyWhenNoVmsAreAvailableToStart()
        {
            var testCase = new BotTestCase()
            {
                Action = "shutdown all vms",
                ExpectedReply = "No virtual machines that can be shut down were found in the current subscription.",
            };

            await TestRunner.RunTestCase(testCase);
        }

        [TestMethod]
        [TestCategory("VMs")]
        public async Task ShouldStopAllVmsFromSpecifiedResourceGroup()
        {
            var resourceGroup = this.TestContext.GetResourceGroup();

            var step1 = new BotTestCase()
            {
                Action = $"stop all vms from {resourceGroup} resource group",
                ExpectedReply = "You are trying to stop the following virtual machines",
            };

            var step2 = new BotTestCase()
            {
                Action = "Yes",
                ExpectedReply = "Stopping the following virtual machines",
            };

            var completionTestCase = new BotTestCase()
            {
                ExpectedReply = "virtual machine was stopped successfully",
            };

            var steps = new List<BotTestCase> { step1, step2 };

            await TestRunner.RunTestCases(steps, completionTestCase, 2);
        }

        [TestMethod]
        [TestCategory("VMs")]
        public async Task StopAllVmsFromSpecifiedResourceGroupShouldNotifyWhenNoVmsInTheResourceGroupAreAvailableToStop()
        {
            var resourceGroup = this.TestContext.GetResourceGroup();

            var testCase = new BotTestCase()
            {
                Action = $"stop all vms from {resourceGroup} resource group",
                ExpectedReply = $"No virtual machines that can be stopped were found in the {resourceGroup} resource group of the current subscription.",
            };

            await TestRunner.RunTestCase(testCase);
        }

        [TestMethod]
        [TestCategory("Parallel")]
        public async Task StopAllVmsFromSpecifiedResourceGroupShouldNotifyWhenTheSpecifiedResourceGroupDoesNotExist()
        {
            var resourceGroup = "NOTFOUND";

            var testCase = new BotTestCase()
            {
                Action = $"stop all vms from {resourceGroup} resource group",
                ExpectedReply = $"The {resourceGroup} resource group doesn't contain VMs or doesn't exist in the current subscription.",
            };

            await TestRunner.RunTestCase(testCase);
        }

        [TestMethod]
        [TestCategory("VMs")]
        public async Task ShouldStopAllVms()
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
                ExpectedReply = "virtual machine was stopped successfully",
            };

            var steps = new List<BotTestCase> { step1, step2 };

            await TestRunner.RunTestCases(steps, completionTestCase, 1);
        }

        [TestMethod]
        [TestCategory("VMs")]
        public async Task ShouldStartAllVmsFromSpecifiedResourceGroup()
        {
            var resourceGroup = this.TestContext.GetResourceGroup();

            var step1 = new BotTestCase()
            {
                Action = $"start all vms from {resourceGroup} resource group",
                ExpectedReply = "You are trying to start the following virtual machines",
            };

            var step2 = new BotTestCase()
            {
                Action = "Yes",
                ExpectedReply = "Starting the following virtual machines",
            };

            var completionTestCase = new BotTestCase()
            {
                ExpectedReply = "virtual machine was started successfully",
            };

            var steps = new List<BotTestCase> { step1, step2 };

            await TestRunner.RunTestCases(steps, completionTestCase, 2);
        }

        [TestMethod]
        [TestCategory("VMs")]
        public async Task StartAllVmsFromSpecifiedResourceGroupShouldNotifyWhenNoVmsInTheResourceGroupAreAvailableToStart()
        {
            var resourceGroup = this.TestContext.GetResourceGroup();

            var testCase = new BotTestCase()
            {
                Action = $"start all vms from {resourceGroup} resource group",
                ExpectedReply = $"No virtual machines that can be started were found in the {resourceGroup} resource group of the current subscription.",
            };

            await TestRunner.RunTestCase(testCase);
        }

        [TestMethod]
        [TestCategory("Parallel")]
        public async Task StartAllVmsFromSpecifiedResourceGroupShouldNotifyWhenTheSpecifiedResourceGroupDoesNotExist()
        {
            var resourceGroup = "NOTFOUND";

            var testCase = new BotTestCase()
            {
                Action = $"start all vms from {resourceGroup} resource group",
                ExpectedReply = $"The {resourceGroup} resource group doesn't contain VMs or doesn't exist in the current subscription.",
            };

            await TestRunner.RunTestCase(testCase);
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
