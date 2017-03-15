using AuthBot;
using AzureBot.Domain;
using AzureBot.Forms;
using AzureBot.Helpers;
using AzureBot.Models;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.FormFlow;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AzureBot.Services.VMs.Forms;
using System.Configuration;
using Microsoft.Bot.Connector;

namespace AzureBot.Dialogs
{
    [LuisModel("836166b9-d8c1-4185-9515-0ebfbf3226dc", "110c81d75bdb4f918a991696cd09f66b")]
    [Serializable]
    public class VMDialog : AzureBotLuisDialog<string>
    {
        private static Lazy<string> resourceId = new Lazy<string>(() => ConfigurationManager.AppSettings["ActiveDirectory.ResourceId"]);

        [LuisIntent("ListVms")]
        public async Task ListVmsAsync(IDialogContext context, LuisResult result)
        {
            var accessToken = await context.GetAccessToken(resourceId.Value);
            if (string.IsNullOrEmpty(accessToken))
            {
                return;
            }

            var subscriptionId = context.GetSubscriptionId();

            var virtualMachines = (await new VMDomain().ListVirtualMachinesAsync(accessToken, subscriptionId)).ToList();
            if (virtualMachines.Any())
            {
                var virtualMachinesText = virtualMachines.Aggregate(
                     string.Empty,
                    (current, next) =>
                    {
                        return current += $"\n\r• {next}";
                    });

                await context.PostAsync($"Available VMs are:\r\n {virtualMachinesText}");
            }
            else
            {
                await context.PostAsync("No virtual machines were found in the current subscription.");
            }

            context.Done<string>(null);
        }

        [LuisIntent("None")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            context.Done(result.Query);
        }

        [LuisIntent("StartVm")]
        public async Task StartVmAsync(IDialogContext context, LuisResult result)
        {
            await this.ProcessVirtualMachineActionAsync(context, result, Operations.Start, this.StartVirtualMachineFormComplete);
        }

        [LuisIntent("StartAllVms")]
        public async Task StartAllVmsAsync(IDialogContext context, LuisResult result)
        {
            await this.ProcessAllVirtualMachinesActionAsync(context, result, Operations.Start, this.StartAllVirtualMachinesFormComplete);
        }

        [LuisIntent("StopVm")]
        public async Task StopVmAsync(IDialogContext context, LuisResult result)
        {
            await this.ProcessVirtualMachineActionAsync(context, result, Operations.Stop, this.StopVirtualMachineFormComplete);
        }

        [LuisIntent("StopAllVms")]
        public async Task StopAllVmsAsync(IDialogContext context, LuisResult result)
        {
            await this.ProcessAllVirtualMachinesActionAsync(context, result, Operations.Stop, this.StopAllVirtualMachinesFormComplete);
        }

        [LuisIntent("ShutdownVm")]
        public async Task ShutdownVmAsync(IDialogContext context, LuisResult result)
        {
            await this.ProcessVirtualMachineActionAsync(context, result, Operations.Shutdown, this.ShutdownVirtualMachineFormComplete);
        }

        [LuisIntent("ShutdownAllVms")]
        public async Task ShutdownAllVmsAsync(IDialogContext context, LuisResult result)
        {
            await this.ProcessAllVirtualMachinesActionAsync(context, result, Operations.Shutdown, this.ShutdownAllVirtualMachinesFormComplete);
        }

        private async Task ProcessVirtualMachineActionAsync(IDialogContext context, LuisResult result,
            Operations operation, ResumeAfter<VirtualMachineFormState> resume)
        {
            EntityRecommendation virtualMachineEntity;

            // retrieve the list virtual machines from the subscription
            var accessToken = await context.GetAccessToken(resourceId.Value);
            if (string.IsNullOrEmpty(accessToken))
            {
                return;
            }

            var subscriptionId = context.GetSubscriptionId();
            var availableVMs = (await new VMDomain().ListVirtualMachinesAsync(accessToken, subscriptionId)).ToList();

            // check if the user specified a virtual machine name in the command
            if (result.TryFindEntity("VirtualMachine", out virtualMachineEntity))
            {
                // obtain the name specified by the user - text in LUIS result is different
                var virtualMachineName = virtualMachineEntity.GetEntityOriginalText(result.Query);

                // ensure that the virtual machine exists in the subscription
                var selectedVM = availableVMs.FirstOrDefault(p => p.Name.Equals(virtualMachineName, StringComparison.InvariantCultureIgnoreCase));
                if (selectedVM == null)
                {
                    await context.PostAsync($"The '{virtualMachineName}' virtual machine was not found in the current subscription.");
                    context.Done<string>(null);
                    return;
                }

                // ensure that the virtual machine is in the correct power state for the requested operation
                if ((operation == Operations.Start && (selectedVM.PowerState == VirtualMachinePowerState.Starting || selectedVM.PowerState == VirtualMachinePowerState.Running))
                   || (operation == Operations.Shutdown && (selectedVM.PowerState == VirtualMachinePowerState.Stopping || selectedVM.PowerState == VirtualMachinePowerState.Stopped))
                   || (operation == Operations.Stop && (selectedVM.PowerState == VirtualMachinePowerState.Deallocating || selectedVM.PowerState == VirtualMachinePowerState.Deallocated)))
                {
                    var powerState = selectedVM.PowerState.ToString().ToLower();
                    await context.PostAsync($"The '{virtualMachineName}' virtual machine is already {powerState}.");
                    context.Done<string>(null);
                    return;
                }

                virtualMachineEntity.Entity = selectedVM.Name;
            }

            // retrieve the list of VMs that are in the correct power state
            var validPowerStates = VirtualMachineHelper.RetrieveValidPowerStateByOperation(operation);

            var candidateVMs = availableVMs.Where(vm => validPowerStates.Contains(vm.PowerState)).ToList();
            if (candidateVMs.Any())
            {
                // prompt the user to select a VM from the list
                var form = new FormDialog<VirtualMachineFormState>(
                    new VirtualMachineFormState(candidateVMs, operation),
                    VMForms.BuildVirtualMachinesForm,
                    FormOptions.PromptInStart,
                    result.Entities);

                context.Call(form, resume);
            }
            else
            {
                var operationText = VirtualMachineHelper.RetrieveOperationTextByOperation(operation);
                await context.PostAsync($"No virtual machines that can be {operationText} were found in the current subscription.");
                context.Done<string>(null);
                //context.Wait(this.MessageReceived);
            }
        }

        private async Task ProcessAllVirtualMachinesActionAsync(IDialogContext context, LuisResult result,
           Operations operation, ResumeAfter<AllVirtualMachinesFormState> resume)
        {
            EntityRecommendation resourceGroupEntity;

            var accessToken = await context.GetAccessToken(resourceId.Value);
            if (string.IsNullOrEmpty(accessToken))
            {
                return;
            }

            var subscriptionId = context.GetSubscriptionId();
            var availableVMs = (await new VMDomain().ListVirtualMachinesAsync(accessToken, subscriptionId)).ToList();

            // retrieve the list of VMs that are in the correct power state
            var validPowerStates = VirtualMachineHelper.RetrieveValidPowerStateByOperation(operation);
            IEnumerable<VirtualMachine> candidateVMs = null;

            if (result.TryFindEntity("ResourceGroup", out resourceGroupEntity))
            {
                // obtain the name specified by the user - text in LUIS result is different
                var resourceGroup = resourceGroupEntity.GetEntityOriginalText(result.Query);

                candidateVMs = availableVMs.Where(vm => vm.ResourceGroup.Equals(resourceGroup, StringComparison.InvariantCultureIgnoreCase)).ToList();

                if (candidateVMs == null || !candidateVMs.Any())
                {
                    var operationText = VirtualMachineHelper.RetrieveOperationTextByOperation(operation);
                    await context.PostAsync($"The {resourceGroup} resource group doesn't contain VMs or doesn't exist in the current subscription.");
                    context.Done<string>(null);
                    return;
                }

                candidateVMs = candidateVMs.Where(vm => validPowerStates.Contains(vm.PowerState)).ToList();

                if (candidateVMs == null || !candidateVMs.Any())
                {
                    var operationText = VirtualMachineHelper.RetrieveOperationTextByOperation(operation);
                    await context.PostAsync($"No virtual machines that can be {operationText} were found in the {resourceGroup} resource group of the current subscription.");
                    context.Done<string>(null);
                    return;
                }
            }
            else
            {
                candidateVMs = availableVMs.Where(vm => validPowerStates.Contains(vm.PowerState)).ToList();

                if (!candidateVMs.Any())
                {
                    var operationText = VirtualMachineHelper.RetrieveOperationTextByOperation(operation);
                    await context.PostAsync($"No virtual machines that can be {operationText} were found in the current subscription.");
                    context.Done<string>(null);
                    return;
                }
            }

            // prompt the user to select a VM from the list
            var form = new FormDialog<AllVirtualMachinesFormState>(
                new AllVirtualMachinesFormState(candidateVMs, operation),
                VMForms.BuildAllVirtualMachinesForm,
                FormOptions.PromptInStart,
                null);

            context.Call(form, resume);
        }

        private async Task StartVirtualMachineFormComplete(IDialogContext context, IAwaitable<VirtualMachineFormState> result)
        {
            Func<string, string> preMessageHandler = vm => $"Starting the '{vm}' virtual machine...";

            Func<bool, string, string> notifyLongRunningOperationHandler = (operationStatus, vmName) =>
            {
                var statusMessage = operationStatus ? "was started successfully" : "failed to start";
                return $"The '{vmName}' virtual machine {statusMessage}.";
            };

            await this.ProcessVirtualMachineFormComplete(
                context,
                result,
                new VMDomain().StartVirtualMachineAsync,
                preMessageHandler,
                notifyLongRunningOperationHandler);
        }

        private async Task StopVirtualMachineFormComplete(IDialogContext context, IAwaitable<VirtualMachineFormState> result)
        {
            Func<string, string> preMessageHandler = vm => $"Stopping the '{vm}' virtual machine...";

            Func<bool, string, string> notifyLongRunningOperationHandler = (operationStatus, vmName) =>
            {
                var statusMessage = operationStatus ? "was stopped successfully" : "failed to stop";
                return $"The '{vmName}' virtual machine {statusMessage}.";
            };

            await this.ProcessVirtualMachineFormComplete(
                context,
                result,
                new VMDomain().DeallocateVirtualMachineAsync,
                preMessageHandler,
                notifyLongRunningOperationHandler);
        }

        private async Task ShutdownVirtualMachineFormComplete(IDialogContext context, IAwaitable<VirtualMachineFormState> result)
        {
            Func<string, string> preMessageHandler = vm => $"Shutting down the '{vm}' virtual machine...";

            Func<bool, string, string> notifyLongRunningOperationHandler = (operationStatus, vmName) =>
            {
                var statusMessage = operationStatus ? "was shut down successfully" : "failed to shutdown";
                return $"The '{vmName}' virtual machine {statusMessage}.";
            };

            await this.ProcessVirtualMachineFormComplete(
                context,
                result,
                new VMDomain().PowerOffVirtualMachineAsync,
                preMessageHandler,
                notifyLongRunningOperationHandler);
        }

        private async Task ProcessVirtualMachineFormComplete(IDialogContext context, IAwaitable<VirtualMachineFormState> result,
            Func<string, string, string, string, Task<bool>> operationHandler, Func<string, string> preMessageHandler,
            Func<bool, string, string> notifyLongRunningOperationHandler)
        {
            try
            {
                var virtualMachineFormState = await result;
                var vm = virtualMachineFormState.SelectedVM;

                await context.PostAsync(preMessageHandler(vm.Name));

                var accessToken = await context.GetAccessToken(resourceId.Value);
                if (string.IsNullOrEmpty(accessToken))
                {
                    return;
                }

                operationHandler(accessToken, vm.SubscriptionId, vm.ResourceGroup, vm.Name)
                    .NotifyLongRunningOperation(context, notifyLongRunningOperationHandler, vm.Name);
            }
            catch (FormCanceledException<VirtualMachineFormState>)
            {
                await context.PostAsync("You have canceled the operation. What would you like to do next?");
            }

            context.Done<string>(null);
        }

        private async Task StartAllVirtualMachinesFormComplete(IDialogContext context, IAwaitable<AllVirtualMachinesFormState> result)
        {
            Func<string, string> preMessageHandler = vms => $"Starting the following virtual machines: {vms}";

            Func<bool, string, string> notifyLongRunningOperationHandler = (operationStatus, vmName) =>
            {
                var statusMessage = operationStatus ? "was started successfully" : "failed to start";
                return $"The '{vmName}' virtual machine {statusMessage}.";
            };

            await this.ProcessAllVirtualMachinesFormComplete(
                context,
                result,
                new VMDomain().StartVirtualMachineAsync,
                preMessageHandler,
                notifyLongRunningOperationHandler);
        }

        private async Task StopAllVirtualMachinesFormComplete(IDialogContext context, IAwaitable<AllVirtualMachinesFormState> result)
        {
            Func<string, string> preMessageHandler = vms => $"Stopping the following virtual machines: {vms}";

            Func<bool, string, string> notifyLongRunningOperationHandler = (operationStatus, vmName) =>
            {
                var statusMessage = operationStatus ? "was stopped successfully" : "failed to stop";
                return $"The '{vmName}' virtual machine {statusMessage}.";
            };

            await this.ProcessAllVirtualMachinesFormComplete(
                context,
                result,
                new VMDomain().DeallocateVirtualMachineAsync,
                preMessageHandler,
                notifyLongRunningOperationHandler);
        }

        private async Task ShutdownAllVirtualMachinesFormComplete(IDialogContext context, IAwaitable<AllVirtualMachinesFormState> result)
        {
            Func<string, string> preMessageHandler = vms => $"Shutting down the following virtual machines: {vms}";

            Func<bool, string, string> notifyLongRunningOperationHandler = (operationStatus, vmName) =>
            {
                var statusMessage = operationStatus ? "was shut down successfully" : "failed to shutdown";
                return $"The '{vmName}' virtual machine {statusMessage}.";
            };

            await this.ProcessAllVirtualMachinesFormComplete(
                context,
                result,
                new VMDomain().PowerOffVirtualMachineAsync,
                preMessageHandler,
                notifyLongRunningOperationHandler);
        }

        private async Task ProcessAllVirtualMachinesFormComplete(IDialogContext context, IAwaitable<AllVirtualMachinesFormState> result,
            Func<string, string, string, string, Task<bool>> operationHandler, Func<string, string> preMessageHandler,
            Func<bool, string, string> notifyLongRunningOperationHandler)
        {
            try
            {
                var allVirtualMachineFormState = await result;

                await context.PostAsync(preMessageHandler(allVirtualMachineFormState.VirtualMachines));

                var accessToken = await context.GetAccessToken(resourceId.Value);
                if (string.IsNullOrEmpty(accessToken))
                {
                    return;
                }

                Parallel.ForEach(
                    allVirtualMachineFormState.AvailableVMs,
                    vm =>
                    {
                        operationHandler(accessToken, vm.SubscriptionId, vm.ResourceGroup, vm.Name)
                            .NotifyLongRunningOperation(context, notifyLongRunningOperationHandler, vm.Name);
                    });
            }
            catch (FormCanceledException<AllVirtualMachinesFormState>)
            {
                await context.PostAsync("You have canceled the operation. What would you like to do next?");
            }

            context.Done<string>(null);
        }

    }
}
