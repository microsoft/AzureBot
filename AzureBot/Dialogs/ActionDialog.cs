namespace AzureBot.Dialogs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Azure.Management.Models;
    using Azure.Management.ResourceManagement;
    using Forms;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.FormFlow;
    using Microsoft.Bot.Builder.Luis;
    using Microsoft.Bot.Builder.Luis.Models;
    using Microsoft.Bot.Connector;

    [LuisModel("1b58a513-e98a-4a13-a5c4-f61ac6dc6c84", "0e64d2ae951547f692182b4ae74262cb")]
    [Serializable]
    public class ActionDialog : LuisDialog<string>
    {
        [LuisIntent("")]
        [LuisIntent("None")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            string message = $"Sorry, I did not understand '{result.Query}'. Type 'help' if you need assistance.";

            await context.PostAsync(message);

            context.Wait(this.MessageReceived);
        }

        [LuisIntent("Help")]
        public async Task Help(IDialogContext context, LuisResult result)
        {
            string message = "Hello! You can use the Azure Bot to: \n";
            message += $"* List and select an Azure subscription (e.g. 'list all subscriptions', 'use the QA subscription')\n";
            message += $"* List, start and stop your virtual machines (e.g. 'show me my VMs', 'start vm serverProd01', 'stop the devEnv01 virtual machine')\n";
            message += $"* Start a runbook\n";

            await context.PostAsync(message);

            context.Wait(this.MessageReceived);
        }

        [LuisIntent("ListSubscriptions")]
        public async Task ListSubscriptionsAsync(IDialogContext context, LuisResult result)
        {
            int index = 0;
            var accessToken = await context.GetAccessToken();
            if (string.IsNullOrEmpty(accessToken))
            {
                return;
            }

            var subscriptions = await new AzureRepository().ListSubscriptionsAsync(accessToken);

            var subscriptionsText = subscriptions.Aggregate(
                string.Empty,
                (current, next) =>
                    {
                        index++;
                        return current += $"\r\n{index}. {next.DisplayName}";
                    });

            await context.PostAsync($"Your subscriptions are: {subscriptionsText}");

            context.Wait(this.MessageReceived);
        }

        [LuisIntent("UseSubscription")]
        public async Task UseSubscriptionAsync(IDialogContext context, LuisResult result)
        {
            EntityRecommendation subscriptionEntity;

            var accessToken = await context.GetAccessToken();
            if (string.IsNullOrEmpty(accessToken))
            {
                return;
            }

            var availableSubscriptions = await new AzureRepository().ListSubscriptionsAsync(accessToken);

            // check if the user specified a subscription name in the command
            if (result.TryFindEntity("Subscription", out subscriptionEntity))
            {
                // obtain the name specified by the user - text in LUIS result is different
                var subscriptionName = subscriptionEntity.GetEntityOriginalText(result.Query);

                // ensure that the subscription exists
                var selectedSubscription = availableSubscriptions.FirstOrDefault(p => p.DisplayName == subscriptionName);
                if (selectedSubscription == null)
                {
                    await context.PostAsync($"The '{subscriptionName}' subscription was not found.");
                    context.Wait(this.MessageReceived);
                    return;
                }

                subscriptionEntity.Entity = selectedSubscription.DisplayName;
                subscriptionEntity.Type = "SubscriptionId";
            }

            var formState = new SubscriptionFormState(availableSubscriptions);

            if (availableSubscriptions.Count() == 1)
            {
                formState.SubscriptionId = availableSubscriptions.Single().SubscriptionId;
                formState.DisplayName = availableSubscriptions.Single().DisplayName;
            }

            var form = new FormDialog<SubscriptionFormState>(
                formState,
                EntityForms.BuildSubscriptionForm,
                FormOptions.PromptInStart,
                result.Entities);

            context.Call(form, this.UseSubscriptionFormComplete);
        }

        [LuisIntent("ListVms")]
        public async Task ListVmsAsync(IDialogContext context, LuisResult result)
        {
            var accessToken = await context.GetAccessToken();
            if (string.IsNullOrEmpty(accessToken))
            {
                return;
            }

            var subscriptionId = context.GetSubscriptionId();

            var virtualMachines = (await new AzureRepository().ListVirtualMachinesAsync(accessToken, subscriptionId)).ToList();
            if (virtualMachines.Any())
            {
                var virtualMachinesText = virtualMachines.Aggregate(
                    string.Empty,
                    (current, next) =>
                    {
                        return current += $"\n\r• {next.Name} ({next.PowerState})";
                    });

                await context.PostAsync($"Available VMs are:\r\n {virtualMachinesText}");
            }
            else
            {
                await context.PostAsync("No virtual machines were found in the current subscription.");
            }

            context.Wait(this.MessageReceived);
        }

        [LuisIntent("StartVm")]
        public async Task StartVmAsync(IDialogContext context, LuisResult result)
        {
            await this.ProcessVirtualMachineActionAsync(context, result, Operations.Start, this.StartVirtualMachineFormComplete);
        }

        [LuisIntent("StopVm")]
        public async Task StopVmAsync(IDialogContext context, LuisResult result)
        {
            await this.ProcessVirtualMachineActionAsync(context, result, Operations.Stop, this.StopVirtualMachineFormComplete);
        }

        [LuisIntent("RunRunbook")]
        public async Task StartRunbookAsync(IDialogContext context, LuisResult result)
        {
            var accessToken = await context.GetAccessToken();
            if (string.IsNullOrEmpty(accessToken))
            {
                return;
            }

            var subscriptionId = context.GetSubscriptionId();

            var availableAutomationAccounts = await new AzureRepository().ListAutomationAccountsAsync(accessToken, subscriptionId);

            var form = new FormDialog<RunbookFormState>(
                new RunbookFormState(availableAutomationAccounts),
                EntityForms.BuildRunbookForm,
                FormOptions.PromptInStart,
                result.Entities);
            context.Call(form, this.StartRunbookParametersAsync);
        }

        protected override async Task MessageReceived(IDialogContext context, IAwaitable<Message> item)
        {
            var message = await item;
            context.PerUserInConversationData.SetValue(ContextConstants.CurrentMessageFromKey, message.From);
            context.PerUserInConversationData.SetValue(ContextConstants.CurrentMessageToKey, message.To);

            if (string.IsNullOrEmpty(await context.GetAccessToken()))
            {
                await context.Forward(new AzureAuthDialog(), this.ResumeAfterAuth, message, CancellationToken.None);
            }
            else if (string.IsNullOrEmpty(context.GetSubscriptionId()))
            {
                await this.UseSubscriptionAsync(context, new LuisResult());
            }
            else
            {
                await base.MessageReceived(context, item);
            }
        }

        private async Task ResumeAfterAuth(IDialogContext context, IAwaitable<string> result)
        {
            var message = await result;

            await context.PostAsync(message);

            await this.UseSubscriptionAsync(context, new LuisResult());
        }

        private async Task StartRunbookParametersAsync(IDialogContext context, IAwaitable<RunbookFormState> result)
        {
            try
            {
                var runbookFormState = await result;
                context.PerUserInConversationData.SetValue(ContextConstants.RunbookFormStateKey, runbookFormState);

                await this.RunbookParametersFormComplete(context, null);
            }
            catch (FormCanceledException<RunbookFormState> e)
            {
                string reply;

                if (e.InnerException == null)
                {
                    reply = "You have canceled the operation. What would you like to do next?";
                }
                else
                {
                    reply = $"Oops! Something went wrong :(. Technical Details: {e.InnerException.Message}";
                }

                await context.PostAsync(reply);

                context.Wait(this.MessageReceived);
            }
        }

        private async Task RunbookParametersFormComplete(IDialogContext context, RunbookParameterFormState runbookParameterFormState)
        {
            var runbookFormState = context.PerUserInConversationData.Get<RunbookFormState>(ContextConstants.RunbookFormStateKey);
            if (runbookParameterFormState != null)
            {
                runbookFormState.RunbookParameters.Add(runbookParameterFormState);
                context.PerUserInConversationData.SetValue(ContextConstants.RunbookFormStateKey, runbookFormState);
            }

            var nextRunbookParameter = runbookFormState.SelectedRunbook.RunbookParameters.OrderBy(param => param.Position).FirstOrDefault(
                availableParam => !runbookFormState.RunbookParameters.Any(stateParam => availableParam.ParameterName == stateParam.ParameterName));

            if (nextRunbookParameter == null)
            {
                await this.RunbookFormComplete(context, runbookFormState);
                return;
            }

            var form = new FormDialog<RunbookParameterFormState>(
                new RunbookParameterFormState { ParameterName = nextRunbookParameter.ParameterName },
                EntityForms.BuildRunbookParametersForm,
                FormOptions.PromptInStart);

            context.Call(form, this.RunbookParameterFormComplete);
        }

        private async Task RunbookParameterFormComplete(IDialogContext context, IAwaitable<RunbookParameterFormState> result)
        {
            try
            {
                var runbookParameterFormState = await result;

                await this.RunbookParametersFormComplete(context, runbookParameterFormState);
            }
            catch (FormCanceledException<RunbookParameterFormState> e)
            {
                string reply;

                if (e.InnerException == null)
                {
                    reply = "You have canceled the operation. What would you like to do next?";
                }
                else
                {
                    reply = $"Oops! Something went wrong :(. Technical Details: {e.InnerException.Message}";
                }

                await context.PostAsync(reply);

                context.Wait(this.MessageReceived);
            }
        }

        private async Task RunbookFormComplete(IDialogContext context, RunbookFormState runbookFormState)
        {
            try
            {
                var accessToken = await context.GetAccessToken();
                if (string.IsNullOrEmpty(accessToken))
                {
                    return;
                }

                await context.PostAsync($"Running the '{runbookFormState.RunbookName}' runbook in '{runbookFormState.AutomationAccountName}' automation account.");

                await new AzureRepository().StartRunbookAsync(
                    accessToken,
                    runbookFormState.SelectedAutomationAccount.SubscriptionId,
                    runbookFormState.SelectedAutomationAccount.ResourceGroup,
                    runbookFormState.SelectedAutomationAccount.AutomationAccountName,
                    runbookFormState.RunbookName,
                    runbookFormState.RunbookParameters.ToDictionary(param => param.ParameterName, param => param.ParameterValue));
            }
            catch (Exception e)
            {
                await context.PostAsync($"Oops! Something went wrong :(. Technical Details: {e.InnerException.Message}");
            }

            context.Wait(this.MessageReceived);
        }

        private async Task ProcessVirtualMachineActionAsync(
            IDialogContext context,
            LuisResult result,
            Operations operation,
            ResumeAfter<VirtualMachineFormState> resume)
        {
            EntityRecommendation virtualMachine;
            List<EntityRecommendation> entities = new List<EntityRecommendation>();

            // retrieve the list virtual machines from the subscription
            var accessToken = await context.GetAccessToken();
            if (string.IsNullOrEmpty(accessToken))
            {
                return;
            }

            var subscriptionId = context.GetSubscriptionId();
            var availableVMs = (await new AzureRepository().ListVirtualMachinesAsync(accessToken, subscriptionId)).ToList();

            // check if the user specified a virtual machine name in the command
            if (result.TryFindEntity("VirtualMachine", out virtualMachine))
            {
                // obtain the name specified by the user - text in LUIS result is different
                var virtualMachineName = virtualMachine.GetEntityOriginalText(result.Query);

                // ensure that the virtual machine exists in the subscription
                var selectedVM = availableVMs.FirstOrDefault(p => p.Name == virtualMachineName);
                if (selectedVM == null)
                {
                    await context.PostAsync($"The '{virtualMachineName}' virtual machine was not found in the current subscription.");
                    context.Wait(this.MessageReceived);
                    return;
                }

                // ensure that the virtual machine is in the correct power state for the requested operation
                if ((operation == Operations.Start && (selectedVM.PowerState == VirtualMachinePowerState.Starting || selectedVM.PowerState == VirtualMachinePowerState.Running))
                   || (operation == Operations.Stop && (selectedVM.PowerState == VirtualMachinePowerState.Stopping || selectedVM.PowerState == VirtualMachinePowerState.Stopped)))
                {
                    var powerState = selectedVM.PowerState.ToString().ToLower();
                    await context.PostAsync($"The '{virtualMachineName}' virtual machine is already {powerState}.");
                    context.Wait(this.MessageReceived);
                    return;
                }

                // add the virtual machine name to the list of entities passed to the form
                entities.Add(new EntityRecommendation(
                            role: virtualMachine.Role,
                            entity: virtualMachineName,
                            type: virtualMachine.Type,
                            startIndex: virtualMachine.StartIndex,
                            endIndex: virtualMachine.EndIndex,
                            score: virtualMachine.Score,
                            resolution: virtualMachine.Resolution));
            }

            // retrieve the list of VMs that are in the correct power state
            var validPowerState = operation == Operations.Start ? VirtualMachinePowerState.Stopped : VirtualMachinePowerState.Running;
            var candidateVMs = availableVMs.Where(vm => vm.PowerState == validPowerState).ToList();
            if (candidateVMs.Any())
            {
                // prompt the user to select a VM from the list
                var form = new FormDialog<VirtualMachineFormState>(
                    new VirtualMachineFormState(candidateVMs, operation),
                    EntityForms.BuildVirtualMachinesForm,
                    FormOptions.PromptInStart,
                    entities);

                context.Call(form, resume);
            }
            else
            {
                var operationText = operation == Operations.Start ? "started" : "stopped";
                await context.PostAsync($"No virtual machines that can be {operationText} were found in the current subscription.");
                context.Wait(this.MessageReceived);
            }
        }

        private async Task StartVirtualMachineFormComplete(IDialogContext context, IAwaitable<VirtualMachineFormState> result)
        {
            try
            {
                var virtualMachineFormState = await result;

                await context.PostAsync($"Starting the '{virtualMachineFormState.VirtualMachine}' virtual machine...");

                var accessToken = await context.GetAccessToken();
                if (string.IsNullOrEmpty(accessToken))
                {
                    return;
                }

                new AzureRepository()
                    .StartVirtualMachineAsync(
                        accessToken,
                        virtualMachineFormState.SelectedVM.SubscriptionId,
                        virtualMachineFormState.SelectedVM.ResourceGroup,
                        virtualMachineFormState.SelectedVM.Name)
                    .NotifyLongRunningOperation(
                        context,
                        (operationStatus) =>
                        {
                            var statusMessage = operationStatus ? "was started successfully" : "failed to start";
                            return $"The '{virtualMachineFormState.VirtualMachine}' virtual machine {statusMessage}.";
                        });
            }
            catch (FormCanceledException<VirtualMachineFormState>)
            {
                await context.PostAsync("You have canceled the operation. What would you like to do next?");
            }

            context.Wait(this.MessageReceived);
        }

        private async Task StopVirtualMachineFormComplete(IDialogContext context, IAwaitable<VirtualMachineFormState> result)
        {
            try
            {
                var virtualMachineFormState = await result;

                await context.PostAsync($"Stopping the '{virtualMachineFormState.VirtualMachine}' virtual machine...");

                var selectedVM = virtualMachineFormState.SelectedVM;
                var accessToken = await context.GetAccessToken();
                if (string.IsNullOrEmpty(accessToken))
                {
                    return;
                }

                new AzureRepository()
                    .StopVirtualMachineAsync(
                        accessToken,
                        virtualMachineFormState.SelectedVM.SubscriptionId,
                        virtualMachineFormState.SelectedVM.ResourceGroup,
                        virtualMachineFormState.SelectedVM.Name)
                    .NotifyLongRunningOperation(
                        context,
                        (operationStatus) =>
                        {
                            var statusMessage = operationStatus ? "was stopped successfully" : "failed to stop";
                            return $"The '{virtualMachineFormState.VirtualMachine}' virtual machine {statusMessage}.";
                        });
            }
            catch (FormCanceledException<VirtualMachineFormState>)
            {
                await context.PostAsync("You have canceled the operation. What would you like to do next?");
            }

            context.Wait(this.MessageReceived);
        }

        private async Task UseSubscriptionFormComplete(IDialogContext context, IAwaitable<SubscriptionFormState> result)
        {
            try
            {
                var subscriptionFormState = await result;
                if (!string.IsNullOrEmpty(subscriptionFormState.SubscriptionId))
                {
                    var selectedSubscription = subscriptionFormState.AvailableSubscriptions.Single(sub => sub.SubscriptionId == subscriptionFormState.SubscriptionId);
                    context.StoreSubscriptionId(subscriptionFormState.SubscriptionId);
                    await context.PostAsync($"Setting {selectedSubscription.DisplayName} as the current subscription.");
                    context.Wait(this.MessageReceived);
                }
                else
                {
                    PromptDialog.Confirm(
                        context,
                        this.OnLogoutRequested,
                        "Oops! You don't have any Azure subscriptions under the account you used to log in. To continue using the bot, log in with a different account. Do you want to log out and start over?",
                        "Didn't get that!",
                        promptStyle: PromptStyle.None);
                }
            }
            catch (FormCanceledException<SubscriptionFormState> e)
            {
                string reply;

                if (e.InnerException == null)
                {
                    reply = "You have canceled the operation. What would you like to do next?";
                }
                else
                {
                    reply = $"Oops! Something went wrong :(. Technical Details: {e.InnerException.Message}";
                }

                await context.PostAsync(reply);
                context.Wait(this.MessageReceived);
            }
        }

        private async Task OnLogoutRequested(IDialogContext context, IAwaitable<bool> confirmation)
        {
            var result = await confirmation;

            if (result)
            {
                var message = context.MakeMessage();
                message.From = context.PerUserInConversationData.Get<ChannelAccount>(ContextConstants.CurrentMessageFromKey);
                message.To = context.PerUserInConversationData.Get<ChannelAccount>(ContextConstants.CurrentMessageToKey);

                context.Logout();
                await context.Forward(new AzureAuthDialog(), this.ResumeAfterAuth, message, CancellationToken.None);
                return;
            }

            context.Wait(this.MessageReceived);
        }
    }
}