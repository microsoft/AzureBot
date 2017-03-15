using AuthBot;
using AzureBot.Domain;
using AzureBot.Forms;
using AzureBot.Helpers;
using AzureBot.Models;
using AzureBot.Services.Runbooks.Forms;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.FormFlow;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureBot.Dialogs
{
    [LuisModel("6ca45971-e419-4e43-8ba4-71fb486d3ffc", "110c81d75bdb4f918a991696cd09f66b")]
    [Serializable]
    public class AutomationDialog : AzureBotLuisDialog<string>
    {

        [LuisIntent("None")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            context.Done(result.Query);
        }

        private static Lazy<string> resourceId = new Lazy<string>(() => ConfigurationManager.AppSettings["ActiveDirectory.ResourceId"]);
        [LuisIntent("ListRunbooks")]
        public async Task ListRunbooksAsync(IDialogContext context, LuisResult result)
        {
            var accessToken = await context.GetAccessToken(resourceId.Value);
            if (string.IsNullOrEmpty(accessToken))
            {
                return;
            }

            var subscriptionId = context.GetSubscriptionId();

            var automationAccounts = await new AutomationDomain().ListRunbooksAsync(accessToken, subscriptionId);

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

            context.Done<string>(null);
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

            var automationAccounts = await new AutomationDomain().ListAutomationAccountsAsync(accessToken, subscriptionId);
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

            context.Done<string>(null);
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

            var availableAutomationAccounts = await new AutomationDomain().ListRunbooksAsync(accessToken, subscriptionId);

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
                        context.Done<string>(null);
                        return;
                    }

                    var runbook = selectedAutomationAccount.Runbooks.SingleOrDefault(x => x.RunbookName.Equals(runbookName, StringComparison.InvariantCultureIgnoreCase));

                    // ensure that the runbook exists in the specified automation account
                    if (runbook == null)
                    {
                        await context.PostAsync($"The '{runbookName}' runbook was not found in the '{automationAccountName}' automation account.");
                        context.Done<string>(null);
                        return;
                    }

                    if (!runbook.RunbookState.Equals("Published", StringComparison.InvariantCultureIgnoreCase))
                    {
                        await context.PostAsync($"The '{runbookName}' runbook that you are trying to run is not published (State: {runbook.RunbookState}). Please go the Azure Portal and publish the runbook.");
                        context.Done<string>(null);
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
                        context.Done<string>(null);
                        return;
                    }

                    var runbooks = selectedAutomationAccounts.SelectMany(x => x.Runbooks.Where(r => r.RunbookName.Equals(runbookName, StringComparison.InvariantCultureIgnoreCase)
                                                                            && r.RunbookState.Equals("Published", StringComparison.InvariantCultureIgnoreCase)));

                    if (runbooks == null || !runbooks.Any())
                    {
                        await context.PostAsync($"The '{runbookName}' runbook that you are trying to run is not Published. Please go the Azure Portal and publish the runbook.");
                        context.Done<string>(null);
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
                    AutomationForms.BuildRunbookForm,
                    FormOptions.PromptInStart,
                    result.Entities);
                context.Call(form, this.StartRunbookParametersAsync);
            }
            else
            {
                await context.PostAsync($"No automations accounts were found in the current subscription. Please create an Azure automation account or switch to a subscription which has an automation account in it.");
                context.Done<string>(null);
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
                    var automationJob = await new AutomationDomain().GetAutomationJobAsync(accessToken, subscriptionId, job.ResourceGroupName, job.AutomationAccountName, job.JobId, configureAwait: false);
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

            context.Done<string>(null);
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
                        context.Done<string>(null);
                        return;
                    }

                    var jobOutput = await new AutomationDomain().GetAutomationJobOutputAsync(accessToken, subscriptionId, selectedJob.ResourceGroupName, selectedJob.AutomationAccountName, selectedJob.JobId);

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

            context.Done<string>(null);
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

            var availableAutomationAccounts = await new AutomationDomain().ListRunbooksAsync(accessToken, subscriptionId);

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
                    context.Done<string>(null);
                    return;
                }

                if (selectedAutomationAccounts.Count() == 1)
                {
                    var automationAccount = selectedAutomationAccounts.Single();
                    var runbook = automationAccount.Runbooks.Single(r => r.RunbookName.Equals(runbookName, StringComparison.InvariantCultureIgnoreCase));
                    var description = await new AutomationDomain().GetAutomationRunbookDescriptionAsync(accessToken, subscriptionId, automationAccount.ResourceGroup, automationAccount.AutomationAccountName, runbook.RunbookName) ?? "No description";
                    await context.PostAsync(description);
                    context.Done<string>(null);
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
                                var description = await new AutomationDomain().GetAutomationRunbookDescriptionAsync(accessToken, subscriptionId, automationAccount.ResourceGroup, automationAccount.AutomationAccountName, runbook.RunbookName) ?? "No description";

                                message += $"\n\r• {description}";
                            }
                        }
                    }

                    await context.PostAsync(message);
                    context.Done<string>(null);
                }
            }
            else
            {
                await context.PostAsync($"No runbook was specified. Please try again specifying a runbook name.");
                context.Done<string>(null);
            }
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

                context.Done<string>(null);
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
                AutomationForms.BuildRunbookParametersForm,
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

                context.Done<string>(null);
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

                var runbookJob = await new AutomationDomain().StartRunbookAsync(
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
                    new AutomationDomain().GetAutomationJobAsync,
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

            context.Done<string>(null);
        }

        private static async Task CheckLongRunningOperationStatus<T>(IDialogContext context, RunbookJob automationJob, string accessToken,
            Func<string, string, string, string, string, bool, Task<T>> getOperationStatusAsync, Func<T, bool> completionCondition,
            Func<T, T, RunbookJob, string> getOperationStatusMessage, int delayBetweenPoolingInSeconds = 2)
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
    }
}
