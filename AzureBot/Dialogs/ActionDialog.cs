namespace AzureBot.Dialogs
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using AuthBot;
    using AuthBot.Dialogs;
    using Azure.Management.Models;
    using Azure.Management.ResourceManagement;
    using Helpers;
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
        private static Lazy<string> resourceId = new Lazy<string>(() => ConfigurationManager.AppSettings["ActiveDirectory.ResourceId"]);
        private bool serviceUrlSet = false;
        [LuisIntent("")]
        [LuisIntent("None")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            string message = $"Sorry, I did not understand '{result.Query}'. Type 'help' if you need assistance.";

            await context.PostAsync(message);

            context.Wait(MessageReceived);
        }
        [LuisIntent("Help")]
        public async Task Help(IDialogContext context, LuisResult result)
        {
            string message = "";
            var accessToken = await context.GetAccessToken(resourceId.Value);
            if (string.IsNullOrEmpty(accessToken))
            {
                message += $"Hello!\n\n";
            }
            message += "I can help you: \n";
            message += $"* List, Switch and Select an Azure subscription\n";
            message += $"* List, Start, Shutdown (power off your VM, still incurring compute charges), and Stop (deallocates your VM, no charges) your virtual machines\n";
            message += $"* List your automation accounts and your runbooks\n";
            message += $"* Start a runbook, get the description of a runbook, get the status and output of automation jobs\n";
            message += $"* Login and Logout of an Azure Subscription\n\n";

            if (string.IsNullOrEmpty(accessToken))
            {
                message += $"Please type **login** to interact with me for the first time.";
            }
            

            await context.PostAsync(message);

            context.Wait(this.MessageReceived);
        }

        protected override async Task MessageReceived(IDialogContext context, IAwaitable<IMessageActivity> item)
        {
            var message = await item;

            if (!serviceUrlSet)
            {
                context.PrivateConversationData.SetValue("ServiceUrl", message.ServiceUrl);
                serviceUrlSet = true;
            }
            if (message.Text.ToLowerInvariant().Contains("help"))
            {
                await base.MessageReceived(context, item);

                return;
            }

            var accessToken = await context.GetAccessToken(resourceId.Value);

            if (string.IsNullOrEmpty(accessToken))
            {
                if (message.Text.ToLowerInvariant().Contains("login"))
                {
                    await context.Forward(new AzureAuthDialog(resourceId.Value), this.ResumeAfterAuth, message, CancellationToken.None);
                }
                else
                {
                    await this.Help(context, new LuisResult());
                }
            }
            else
            {
                if (string.IsNullOrEmpty(context.GetSubscriptionId()))
                {
                    await this.UseSubscriptionAsync(context, new LuisResult());
                }
                else
                {
                    if (message.Text.ToLowerInvariant().Contains("create"))
                    {
                        await this.CreateResourceAsync(context);//, new LuisResult());
                    }
                    else
                        await base.MessageReceived(context, item);
                }
            }
        }

        private static async Task CheckLongRunningOperationStatus<T>(
            IDialogContext context,
            RunbookJob automationJob,
            string accessToken,
            Func<string, string, string, string, string, bool, Task<T>> getOperationStatusAsync,
            Func<T, bool> completionCondition,
            Func<T, T, RunbookJob, string> getOperationStatusMessage,
            int delayBetweenPoolingInSeconds = 2)
        {
            var lastOperationStatus = default(T);
            do
            {
                var subscriptionId = context.GetSubscriptionId();

                var newOperationStatus = await getOperationStatusAsync(accessToken, subscriptionId, automationJob.ResourceGroupName, automationJob.AutomationAccountName, automationJob.JobId, true).ConfigureAwait(false);

                var message = getOperationStatusMessage(lastOperationStatus, newOperationStatus, automationJob);
                await context.NotifyUser(message);

                await Task.Delay(TimeSpan.FromSeconds(delayBetweenPoolingInSeconds)).ConfigureAwait(false);
                lastOperationStatus = newOperationStatus;
            }
            while (!completionCondition(lastOperationStatus));
        }

        //[LuisIntent("Create")]
        public async Task CreateResourceAsync(IDialogContext context)//, LuisResult result)
        {
            var accessToken = await context.GetAccessToken(resourceId.Value);
            if (string.IsNullOrEmpty(accessToken))
            {
                return;
            }

            var subscriptionId = context.GetSubscriptionId();

            //TODO - Take USer input for below parameters
            var groupName = "hackathonDemo";
            var storageName = "hacky2016";
            var deploymentName = "MyVmDeployment";
            var templateContainer = "templates";
            var templateJson = "azuredeploy.json";
            var parametersJSon = "azuredeploy.parameters.json";
   
            await new AzureRepository().CreateTemplateDeploymentAsync(accessToken, groupName, storageName, deploymentName, subscriptionId, templateContainer, templateJson, parametersJSon);

            await context.PostAsync($"Your VM deployment is initiated. Will let you know once deployment is completed. What's next?");

            context.Wait(this.MessageReceived);

        }

        [LuisIntent("ListSubscriptions")]
        public async Task ListSubscriptionsAsync(IDialogContext context, LuisResult result)
        {
            var accessToken = await context.GetAccessToken(resourceId.Value);
            if (string.IsNullOrEmpty(accessToken))
            {
                return;
            }

            var subscriptions = await new AzureRepository().ListSubscriptionsAsync(accessToken);
            
            int index = 0;
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

        [LuisIntent("CurrentSubscription")]
        public async Task GetCurrentSubscriptionAsync(IDialogContext context, LuisResult result)
        {
            var accessToken = await context.GetAccessToken(resourceId.Value);
            if (string.IsNullOrEmpty(accessToken))
            {
                return;
            }

            var subscriptionId = context.GetSubscriptionId();

            var currentSubscription = await new AzureRepository().GetSubscription(accessToken, subscriptionId);

            await context.PostAsync($"Your current subscription is '{currentSubscription.DisplayName}'.");

            context.Wait(this.MessageReceived);
        }

        [LuisIntent("UseSubscription")]
        public async Task UseSubscriptionAsync(IDialogContext context, LuisResult result)
        {
            EntityRecommendation subscriptionEntity;

            var accessToken = await context.GetAccessToken(resourceId.Value);
            
            if (string.IsNullOrEmpty(accessToken))
            {
                context.Wait(this.MessageReceived);
                return;
            }

            var availableSubscriptions = await new AzureRepository().ListSubscriptionsAsync(accessToken);

            // check if the user specified a subscription name in the command
            if (result.TryFindEntity("Subscription", out subscriptionEntity))
            {
                // obtain the name specified by the user - text in LUIS result is different
                var subscriptionName = subscriptionEntity.GetEntityOriginalText(result.Query);

                // ensure that the subscription exists
                var selectedSubscription = availableSubscriptions.FirstOrDefault(p => p.DisplayName.Equals(subscriptionName, StringComparison.InvariantCultureIgnoreCase));
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
            var accessToken = await context.GetAccessToken(resourceId.Value);
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
                        return current += $"\n\r• {next}";
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

        [LuisIntent("ListRunbooks")]
        public async Task ListRunbooksAsync(IDialogContext context, LuisResult result)
        {
            var accessToken = await context.GetAccessToken(resourceId.Value);
            if (string.IsNullOrEmpty(accessToken))
            {
                return;
            }

            var subscriptionId = context.GetSubscriptionId();

            var automationAccounts = await new AzureRepository().ListRunbooksAsync(accessToken, subscriptionId);

            if (automationAccounts.Any())
            {
                var automationAccountsWithRunbooks = automationAccounts.Where(x => x.Runbooks.Any());

                if (automationAccountsWithRunbooks.Any())
                {
                    var messageText = "Available runbooks are:";

                    var singleAutomationAccount = automationAccountsWithRunbooks.Count() == 1;

                    if (singleAutomationAccount)
                    {
                        messageText = $"Listing all runbooks from {automationAccountsWithRunbooks.Single().AutomationAccountName}";
                    }

                    var runbooksText = automationAccountsWithRunbooks.Aggregate(
                         string.Empty,
                        (current, next) =>
                        {
                            var innerRunbooksText = next.Runbooks.Aggregate(
                                string.Empty,
                                (currentRunbooks, nextRunbook) =>
                                {
                                    return currentRunbooks += $"\n\r• {nextRunbook}";
                                });

                            return current += singleAutomationAccount ? innerRunbooksText : $"\n\r {next.AutomationAccountName}" + innerRunbooksText;
                        });

                    var showDescriptionText = "Type **show runbook <name> description** to get details on any runbook.";
                    await context.PostAsync($"{messageText}:\r\n {runbooksText} \r\n\r\n {showDescriptionText}");
                }
                else
                {
                    await context.PostAsync($"The automation accounts found in the current subscription doesn't have runbooks.");
                }
            }
            else
            {
                await context.PostAsync("No runbooks listed since no automations accounts were found in the current subscription.");
            }

            context.Wait(this.MessageReceived);
        }

        [LuisIntent("ListAutomationAccounts")]
        public async Task ListAutomationAccountsAsync(IDialogContext context, LuisResult result)
        {
            var accessToken = await context.GetAccessToken(resourceId.Value);
            if (string.IsNullOrEmpty(accessToken))
            {
                return;
            }

            var subscriptionId = context.GetSubscriptionId();

            var automationAccounts = await new AzureRepository().ListAutomationAccountsAsync(accessToken, subscriptionId);
            if (automationAccounts.Any())
            {
                var automationAccountsText = automationAccounts.Aggregate(
                     string.Empty,
                    (current, next) =>
                    {
                        return current += $"\n\r• {next.AutomationAccountName}";
                    });

                await context.PostAsync($"Available automations accounts are:\r\n {automationAccountsText}");
            }
            else
            {
                await context.PostAsync("No automations accounts were found in the current subscription.");
            }

            context.Wait(this.MessageReceived);
        }

        [LuisIntent("RunRunbook")]
        public async Task StartRunbookAsync(IDialogContext context, LuisResult result)
        {
            EntityRecommendation runbookEntity;
            var accessToken = await context.GetAccessToken(resourceId.Value);
            if (string.IsNullOrEmpty(accessToken))
            {
                return;
            }

            var subscriptionId = context.GetSubscriptionId();

            var availableAutomationAccounts = await new AzureRepository().ListRunbooksAsync(accessToken, subscriptionId);

            // check if the user specified a runbook name in the command
            if (result.TryFindEntity("Runbook", out runbookEntity))
            {
                // obtain the name specified by the user - text in LUIS result is different
                var runbookName = runbookEntity.GetEntityOriginalText(result.Query);

                EntityRecommendation automationAccountEntity;

                if (result.TryFindEntity("AutomationAccount", out automationAccountEntity))
                {
                    // obtain the name specified by the user - text in LUIS result is different
                    var automationAccountName = automationAccountEntity.GetEntityOriginalText(result.Query);

                    var selectedAutomationAccount = availableAutomationAccounts.SingleOrDefault(x => x.AutomationAccountName.Equals(automationAccountName, StringComparison.InvariantCultureIgnoreCase));

                    if (selectedAutomationAccount == null)
                    {
                        await context.PostAsync($"The '{automationAccountName}' automation account was not found in the current subscription");
                        context.Wait(this.MessageReceived);
                        return;
                    }

                    var runbook = selectedAutomationAccount.Runbooks.SingleOrDefault(x => x.RunbookName.Equals(runbookName, StringComparison.InvariantCultureIgnoreCase));

                    // ensure that the runbook exists in the specified automation account
                    if (runbook == null)
                    {
                        await context.PostAsync($"The '{runbookName}' runbook was not found in the '{automationAccountName}' automation account.");
                        context.Wait(this.MessageReceived);
                        return;
                    }

                    if (!runbook.RunbookState.Equals("Published", StringComparison.InvariantCultureIgnoreCase))
                    {
                        await context.PostAsync($"The '{runbookName}' runbook that you are trying to run is not published (State: {runbook.RunbookState}). Please go the Azure Portal and publish the runbook.");
                        context.Wait(this.MessageReceived);
                        return;
                    }

                    runbookEntity.Entity = runbookName;
                    runbookEntity.Type = "RunbookName";

                    automationAccountEntity.Entity = selectedAutomationAccount.AutomationAccountName;
                    automationAccountEntity.Type = "AutomationAccountName";
                }
                else
                {
                    // ensure that the runbook exists in at least one of the automation accounts
                    var selectedAutomationAccounts = availableAutomationAccounts.Where(x => x.Runbooks.Any(r => r.RunbookName.Equals(runbookName, StringComparison.InvariantCultureIgnoreCase)));

                    if (selectedAutomationAccounts == null || !selectedAutomationAccounts.Any())
                    {
                        await context.PostAsync($"The '{runbookName}' runbook was not found in any of your automation accounts.");
                        context.Wait(this.MessageReceived);
                        return;
                    }

                    var runbooks = selectedAutomationAccounts.SelectMany(x => x.Runbooks.Where(r => r.RunbookName.Equals(runbookName, StringComparison.InvariantCultureIgnoreCase)
                                                                            && r.RunbookState.Equals("Published", StringComparison.InvariantCultureIgnoreCase)));

                    if (runbooks == null || !runbooks.Any())
                    {
                        await context.PostAsync($"The '{runbookName}' runbook that you are trying to run is not Published. Please go the Azure Portal and publish the runbook.");
                        context.Wait(this.MessageReceived);
                        return;
                    }

                    runbookEntity.Entity = runbookName;
                    runbookEntity.Type = "RunbookName";

                    // todo: handle runbooks with same name in different automation accounts
                    availableAutomationAccounts = selectedAutomationAccounts.ToList();
                }
            }

            if (availableAutomationAccounts.Any())
            {
                var formState = new RunbookFormState(availableAutomationAccounts);

                if (availableAutomationAccounts.Count() == 1)
                {
                    formState.AutomationAccountName = availableAutomationAccounts.Single().AutomationAccountName;
                }

                var form = new FormDialog<RunbookFormState>(
                    formState,
                    EntityForms.BuildRunbookForm,
                    FormOptions.PromptInStart,
                    result.Entities);
                context.Call(form, this.StartRunbookParametersAsync);
            }
            else
            {
                await context.PostAsync($"No automations accounts were found in the current subscription. Please create an Azure automation account or switch to a subscription which has an automation account in it.");
                context.Wait(this.MessageReceived);
            }
        }

        [LuisIntent("StatusJob")]
        public async Task StatusJobAsync(IDialogContext context, LuisResult result)
        {
            var accessToken = await context.GetAccessToken(resourceId.Value);
            if (string.IsNullOrEmpty(accessToken))
            {
                return;
            }

            var subscriptionId = context.GetSubscriptionId();

            IList<RunbookJob> automationJobs = context.GetAutomationJobs(subscriptionId);
            if (automationJobs != null && automationJobs.Any())
            {
                var messageBuilder = new StringBuilder();
                messageBuilder.AppendLine("|Id|Runbook|Start Time|End Time|Status|");
                messageBuilder.AppendLine("|---|---|---|---|---|");
                foreach (var job in automationJobs)
                {
                    var automationJob = await new AzureRepository().GetAutomationJobAsync(accessToken, subscriptionId, job.ResourceGroupName, job.AutomationAccountName, job.JobId, configureAwait: false);
                    var startDateTime = automationJob.StartDateTime?.ToString("g") ?? string.Empty;
                    var endDateTime = automationJob.EndDateTime?.ToString("g") ?? string.Empty;
                    var status = automationJob.Status ?? string.Empty;
                    messageBuilder.AppendLine($"|{job.FriendlyJobId}|{automationJob.RunbookName}|{startDateTime}|{endDateTime}|{status}|");
                }

                await context.PostAsync(messageBuilder.ToString());
            }
            else
            {
                await context.PostAsync("No Runbook Jobs were created in the current session. To create a new Runbook Job type: Start Runbook.");
            }

            context.Wait(this.MessageReceived);
        }

        [LuisIntent("ShowJobOutput")]
        public async Task ShowJobOutputAsync(IDialogContext context, LuisResult result)
        {
            EntityRecommendation jobEntity;

            var accessToken = await context.GetAccessToken(resourceId.Value);
            if (string.IsNullOrEmpty(accessToken))
            {
                return;
            }

            var subscriptionId = context.GetSubscriptionId();

            if (result.TryFindEntity("Job", out jobEntity))
            {
                // obtain the name specified by the user -text in LUIS result is different
                var friendlyJobId = jobEntity.GetEntityOriginalText(result.Query);

                IList<RunbookJob> automationJobs = context.GetAutomationJobs(subscriptionId);
                if (automationJobs != null)
                {
                    var selectedJob = automationJobs.SingleOrDefault(x => !string.IsNullOrWhiteSpace(x.FriendlyJobId)
                            && x.FriendlyJobId.Equals(friendlyJobId, StringComparison.InvariantCultureIgnoreCase));

                    if (selectedJob == null)
                    {
                        await context.PostAsync($"The job with id '{friendlyJobId}' was not found.");
                        context.Wait(this.MessageReceived);
                        return;
                    }

                    var jobOutput = await new AzureRepository().GetAutomationJobOutputAsync(accessToken, subscriptionId, selectedJob.ResourceGroupName, selectedJob.AutomationAccountName, selectedJob.JobId);

                    var outputMessage = string.IsNullOrWhiteSpace(jobOutput) ? $"No output for job '{friendlyJobId}'" : jobOutput;

                    await context.PostAsync(outputMessage);
                }
                else
                {
                    await context.PostAsync($"The job with id '{friendlyJobId}' was not found.");
                }
            }
            else
            {
                await context.PostAsync("No runbook job id was specified. Try 'show <jobId> output'.");
            }

            context.Wait(this.MessageReceived);
        }

        [LuisIntent("ShowRunbookDescription")]
        public async Task ShowRunbookDescriptionAsync(IDialogContext context, LuisResult result)
        {
            EntityRecommendation runbookEntity;
            var accessToken = await context.GetAccessToken(resourceId.Value);
            if (string.IsNullOrEmpty(accessToken))
            {
                return;
            }

            var subscriptionId = context.GetSubscriptionId();

            var availableAutomationAccounts = await new AzureRepository().ListRunbooksAsync(accessToken, subscriptionId);

            // check if the user specified a runbook name in the command
            if (result.TryFindEntity("Runbook", out runbookEntity))
            {
                // obtain the name specified by the user - text in LUIS result is different
                var runbookName = runbookEntity.GetEntityOriginalText(result.Query);

                // ensure that the runbook exists in at least one of the automation accounts
                var selectedAutomationAccounts = availableAutomationAccounts.Where(x => x.Runbooks.Any(r => r.RunbookName.Equals(runbookName, StringComparison.InvariantCultureIgnoreCase)));

                if (selectedAutomationAccounts == null || !selectedAutomationAccounts.Any())
                {
                    await context.PostAsync($"The '{runbookName}' runbook was not found in any of your automation accounts.");
                    context.Wait(this.MessageReceived);
                    return;
                }

                if (selectedAutomationAccounts.Count() == 1)
                {
                    var automationAccount = selectedAutomationAccounts.Single();
                    var runbook = automationAccount.Runbooks.Single(r => r.RunbookName.Equals(runbookName, StringComparison.InvariantCultureIgnoreCase));
                    var description = await new AzureRepository().GetAutomationRunbookDescriptionAsync(accessToken, subscriptionId, automationAccount.ResourceGroup, automationAccount.AutomationAccountName, runbook.RunbookName) ?? "No description";
                    await context.PostAsync(description);
                    context.Wait(this.MessageReceived);
                }
                else
                {
                    var message = $"I found the runbook '{runbookName}' in multiple automation accounts. Showing the description of all of them:";

                    foreach (var automationAccount in selectedAutomationAccounts)
                    {
                        message += $"\n\r {automationAccount.AutomationAccountName}";
                        foreach (var runbook in automationAccount.Runbooks)
                        {
                            if (runbook.RunbookName.Equals(runbookName, StringComparison.InvariantCultureIgnoreCase))
                            {
                                var description = await new AzureRepository().GetAutomationRunbookDescriptionAsync(accessToken, subscriptionId, automationAccount.ResourceGroup, automationAccount.AutomationAccountName, runbook.RunbookName) ?? "No description";

                                message += $"\n\r• {description}";
                            }
                        }
                    }

                    await context.PostAsync(message);
                    context.Wait(this.MessageReceived);
                }
            }
            else
            {
                await context.PostAsync($"No runbook was specified. Please try again specifying a runbook name.");
                context.Wait(this.MessageReceived);
            }
        }

        [LuisIntent("Logout")]
        public async Task Logout(IDialogContext context, LuisResult result)
        {
            context.Cleanup();
            await context.Logout();

            context.Wait(this.MessageReceived);
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
                context.StoreRunbookFormState(runbookFormState);

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
            var runbookFormState = context.GetRunbookFormState();
            if (runbookParameterFormState != null)
            {
                runbookFormState.RunbookParameters.Add(runbookParameterFormState);
                context.StoreRunbookFormState(runbookFormState);
            }

            var nextRunbookParameter = runbookFormState.SelectedRunbook.RunbookParameters.OrderBy(param => param.Position).FirstOrDefault(
                availableParam => !runbookFormState.RunbookParameters.Any(stateParam => availableParam.ParameterName == stateParam.ParameterName));

            if (nextRunbookParameter == null)
            {
                context.CleanupRunbookFormState();
                await this.RunbookFormComplete(context, runbookFormState);
                return;
            }

            var formState = new RunbookParameterFormState(nextRunbookParameter.IsMandatory, nextRunbookParameter.Position == 0, runbookFormState.SelectedRunbook.RunbookName)
            {
                ParameterName = nextRunbookParameter.ParameterName
            };

            var form = new FormDialog<RunbookParameterFormState>(
                formState,
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
                context.CleanupRunbookFormState();

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
                var accessToken = await context.GetAccessToken(resourceId.Value);

                if (string.IsNullOrEmpty(accessToken))
                {
                    return;
                }

                var runbookJob = await new AzureRepository().StartRunbookAsync(
                    accessToken,
                    runbookFormState.SelectedAutomationAccount.SubscriptionId,
                    runbookFormState.SelectedAutomationAccount.ResourceGroup,
                    runbookFormState.SelectedAutomationAccount.AutomationAccountName,
                    runbookFormState.RunbookName,
                    runbookFormState.RunbookParameters.Where(param => !string.IsNullOrWhiteSpace(param.ParameterValue))
                        .ToDictionary(param => param.ParameterName, param => param.ParameterValue));

                IList<RunbookJob> automationJobs = context.GetAutomationJobs(runbookFormState.SelectedAutomationAccount.SubscriptionId);
                if (automationJobs == null)
                {
                    runbookJob.FriendlyJobId = AutomationJobsHelper.NextFriendlyJobId(automationJobs);
                    automationJobs = new List<RunbookJob> { runbookJob };
                }
                else
                {
                    runbookJob.FriendlyJobId = AutomationJobsHelper.NextFriendlyJobId(automationJobs);
                    automationJobs.Add(runbookJob);
                }

                context.StoreAutomationJobs(runbookFormState.SelectedAutomationAccount.SubscriptionId, automationJobs);

                await context.PostAsync($"Created Job '{runbookJob.JobId}' for the '{runbookFormState.RunbookName}' runbook in '{runbookFormState.AutomationAccountName}' automation account. You'll receive a message when it is completed.");

                var notCompletedStatusList = new List<string> { "Stopped", "Suspended", "Failed" };
                var completedStatusList = new List<string> { "Completed" };
                var notifyStatusList = new List<string> { "Running" };
                notifyStatusList.AddRange(completedStatusList);
                notifyStatusList.AddRange(notCompletedStatusList);

                accessToken = await context.GetAccessToken(resourceId.Value);

                if (string.IsNullOrEmpty(accessToken))
                {
                    return;
                }

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                CheckLongRunningOperationStatus(
                    context,
                    runbookJob,
                    accessToken,
                    new AzureRepository().GetAutomationJobAsync,
                    rj => rj.EndDateTime.HasValue,
                    (previous, last, job) =>
                    {
                        if (!string.Equals(previous?.Status, last?.Status) && notifyStatusList.Contains(last.Status))
                        {
                            if (notCompletedStatusList.Contains(last.Status))
                            {
                                return $"The runbook '{job.RunbookName}' (job '{job.JobId}') did not complete with status '{last.Status}'. Please go to the Azure Portal for more detailed information on why.";
                            }
                            else if (completedStatusList.Contains(last.Status))
                            {
                                return $"Runbook '{job.RunbookName}' is currently in '{last.Status}' status. Type **show {job.FriendlyJobId} output** to see the output.";
                            }
                            else
                            {
                                return $"Runbook '{job.RunbookName}' job '{job.JobId}' is currently in '{last.Status}' status.";
                            }
                        }

                        return null;
                    });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
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
            EntityRecommendation virtualMachineEntity;

            // retrieve the list virtual machines from the subscription
            var accessToken = await context.GetAccessToken(resourceId.Value);
            if (string.IsNullOrEmpty(accessToken))
            {
                return;
            }

            var subscriptionId = context.GetSubscriptionId();
            var availableVMs = (await new AzureRepository().ListVirtualMachinesAsync(accessToken, subscriptionId)).ToList();

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
                    context.Wait(this.MessageReceived);
                    return;
                }

                // ensure that the virtual machine is in the correct power state for the requested operation
                if ((operation == Operations.Start && (selectedVM.PowerState == VirtualMachinePowerState.Starting || selectedVM.PowerState == VirtualMachinePowerState.Running))
                   || (operation == Operations.Shutdown && (selectedVM.PowerState == VirtualMachinePowerState.Stopping || selectedVM.PowerState == VirtualMachinePowerState.Stopped))
                   || (operation == Operations.Stop && (selectedVM.PowerState == VirtualMachinePowerState.Deallocating || selectedVM.PowerState == VirtualMachinePowerState.Deallocated)))
                {
                    var powerState = selectedVM.PowerState.ToString().ToLower();
                    await context.PostAsync($"The '{virtualMachineName}' virtual machine is already {powerState}.");
                    context.Wait(this.MessageReceived);
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
                    EntityForms.BuildVirtualMachinesForm,
                    FormOptions.PromptInStart,
                    result.Entities);

                context.Call(form, resume);
            }
            else
            {
                var operationText = VirtualMachineHelper.RetrieveOperationTextByOperation(operation);
                await context.PostAsync($"No virtual machines that can be {operationText} were found in the current subscription.");
                context.Wait(this.MessageReceived);
            }
        }

        private async Task ProcessAllVirtualMachinesActionAsync(
           IDialogContext context,
           LuisResult result,
           Operations operation,
           ResumeAfter<AllVirtualMachinesFormState> resume)
        {
            EntityRecommendation resourceGroupEntity;

            var accessToken = await context.GetAccessToken(resourceId.Value);
            if (string.IsNullOrEmpty(accessToken))
            {
                return;
            }

            var subscriptionId = context.GetSubscriptionId();
            var availableVMs = (await new AzureRepository().ListVirtualMachinesAsync(accessToken, subscriptionId)).ToList();

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
                    context.Wait(this.MessageReceived);
                    return;
                }

                candidateVMs = candidateVMs.Where(vm => validPowerStates.Contains(vm.PowerState)).ToList();

                if (candidateVMs == null || !candidateVMs.Any())
                {
                    var operationText = VirtualMachineHelper.RetrieveOperationTextByOperation(operation);
                    await context.PostAsync($"No virtual machines that can be {operationText} were found in the {resourceGroup} resource group of the current subscription.");
                    context.Wait(this.MessageReceived);
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
                    context.Wait(this.MessageReceived);
                    return;
                }
            }

            // prompt the user to select a VM from the list
            var form = new FormDialog<AllVirtualMachinesFormState>(
                new AllVirtualMachinesFormState(candidateVMs, operation),
                EntityForms.BuildAllVirtualMachinesForm,
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
                new AzureRepository().StartVirtualMachineAsync,
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
                new AzureRepository().DeallocateVirtualMachineAsync,
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
                new AzureRepository().PowerOffVirtualMachineAsync,
                preMessageHandler,
                notifyLongRunningOperationHandler);
        }

        private async Task ProcessVirtualMachineFormComplete(
            IDialogContext context,
            IAwaitable<VirtualMachineFormState> result,
            Func<string, string, string, string, Task<bool>> operationHandler,
            Func<string, string> preMessageHandler,
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

            context.Wait(this.MessageReceived);
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
                new AzureRepository().StartVirtualMachineAsync,
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
                new AzureRepository().DeallocateVirtualMachineAsync,
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
                new AzureRepository().PowerOffVirtualMachineAsync,
                preMessageHandler,
                notifyLongRunningOperationHandler);
        }

        private async Task ProcessAllVirtualMachinesFormComplete(
            IDialogContext context,
            IAwaitable<AllVirtualMachinesFormState> result,
            Func<string, string, string, string, Task<bool>> operationHandler,
            Func<string, string> preMessageHandler,
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
                    await context.PostAsync($"Setting {selectedSubscription.DisplayName} as the current subscription. What would you like to do next?");
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
                context.Cleanup();
                await context.Logout();
            }

            context.Wait(this.MessageReceived);
        }
    }
}
