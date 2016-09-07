using AzureBot.Models;
using Microsoft.Azure;
using Microsoft.Azure.Management.Automation;
using Microsoft.Azure.Management.Automation.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AzureBot.Domain
{
    class AutomationDomain
    {
        public async Task<IEnumerable<Models.AutomationAccount>> ListAutomationAccountsAsync(string accessToken, string subscriptionId)
        {
            var credentials = new TokenCloudCredentials(subscriptionId, accessToken);

            using (var automationClient = new AutomationManagementClient(credentials))
            {
                var automationAccountsResult = await automationClient.AutomationAccounts.ListAsync(null).ConfigureAwait(false);
                return automationAccountsResult.AutomationAccounts.Select(account => new Models.AutomationAccount
                {
                    SubscriptionId = subscriptionId,
                    ResourceGroup = Subscription.GetResourceGroup(account.Id),
                    AutomationAccountId = account.Id,
                    AutomationAccountName = account.Name,
                }).ToList();
            }
        }

        public async Task<IEnumerable<Models.AutomationAccount>> ListRunbooksAsync(string accessToken, string subscriptionId)
        {
            var credentials = new TokenCloudCredentials(subscriptionId, accessToken);

            using (var automationClient = new AutomationManagementClient(credentials))
            {
                var automationAccountsResult = await this.ListAutomationAccountsAsync(accessToken, subscriptionId).ConfigureAwait(false);
                var automationAccounts = await Task.WhenAll(
                    automationAccountsResult.Select(
                        async account =>
                        {
                            account.Runbooks = await ListAutomationRunbooks(accessToken, subscriptionId, account.ResourceGroup, account.AutomationAccountName);

                            return account;
                        }).ToList());
                return automationAccounts;
            }
        }

        public async Task<IEnumerable<Models.AutomationAccount>> ListRunbookJobsAsync(string accessToken, string subscriptionId)
        {
            var credentials = new TokenCloudCredentials(subscriptionId, accessToken);

            using (var automationClient = new AutomationManagementClient(credentials))
            {
                var automationAccountsResult = await this.ListAutomationAccountsAsync(accessToken, subscriptionId).ConfigureAwait(false);
                var automationAccounts = await Task.WhenAll(
                    automationAccountsResult.Select(
                        async account =>
                        {
                            account.RunbookJobs = await this.ListAutomationJobs(accessToken, subscriptionId, account.ResourceGroup, account.AutomationAccountName);

                            return account;
                        }).ToList());
                return automationAccounts;
            }
        }

        public async Task<RunbookJob> StartRunbookAsync(
            string accessToken,
            string subscriptionId,
            string resourceGroupName,
            string automationAccountName,
            string runbookName,
            IDictionary<string, string> runbookParameters = null)
        {
            var credentials = new TokenCloudCredentials(subscriptionId, accessToken);

            using (var client = new AutomationManagementClient(credentials))
            {
                var parameters = new JobCreateParameters(
                    new JobCreateProperties(
                        new RunbookAssociationProperty
                        {
                            Name = runbookName
                        })
                    {
                        Parameters = runbookParameters
                    });

                var jobCreateResult = await client.Jobs.CreateAsync(resourceGroupName, automationAccountName, parameters).ConfigureAwait(false);
                return new RunbookJob
                {
                    JobId = jobCreateResult.Job.Properties.JobId.ToString(),
                    StartDateTime = jobCreateResult.Job.Properties.StartTime,
                    EndDateTime = jobCreateResult.Job.Properties.EndTime,
                    Status = jobCreateResult.Job.Properties.Status,
                    ResourceGroupName = resourceGroupName,
                    AutomationAccountName = automationAccountName,
                    RunbookName = runbookName
                };
            }
        }

        public async Task<RunbookJob> GetAutomationJobAsync(string accessToken, string subscriptionId, string resourceGroupName, string automationAccountName, string jobId, bool configureAwait = false)
        {
            var credentials = new TokenCloudCredentials(subscriptionId, accessToken);

            using (var automationClient = new AutomationManagementClient(credentials))
            {
                var automationJobResult = configureAwait ? await automationClient.Jobs.GetAsync(resourceGroupName, automationAccountName, new Guid(jobId)).ConfigureAwait(false) :
                    await automationClient.Jobs.GetAsync(resourceGroupName, automationAccountName, new Guid(jobId));

                var automationJob = new RunbookJob
                {
                    JobId = automationJobResult.Job.Properties.JobId.ToString(),
                    Status = automationJobResult.Job.Properties.Status,
                    RunbookName = automationJobResult.Job.Properties.Runbook?.Name ?? "_(Unknown)_",
                    ResourceGroupName = resourceGroupName,
                    AutomationAccountName = automationAccountName,
                    StartDateTime = automationJobResult.Job.Properties.StartTime,
                    EndDateTime = automationJobResult.Job.Properties.EndTime
                };

                return automationJob;
            }
        }

        public async Task<string> GetAutomationJobOutputAsync(string accessToken, string subscriptionId, string resourceGroupName, string automationAccountName, string jobId)
        {
            var credentials = new TokenCloudCredentials(subscriptionId, accessToken);

            using (var automationClient = new AutomationManagementClient(credentials))
            {
                var jobOutputResponse = await automationClient.Jobs.GetOutputAsync(resourceGroupName, automationAccountName, new Guid(jobId));

                return jobOutputResponse.Output;
            }
        }

        public async Task<string> GetAutomationRunbookDescriptionAsync(
            string accessToken,
            string subscriptionId,
            string resourceGroupName,
            string automationAccountName,
            string runbookName)
        {
            var credentials = new TokenCloudCredentials(subscriptionId, accessToken);

            using (var automationClient = new AutomationManagementClient(credentials))
            {
                var automationRunbookResult = await automationClient.Runbooks.GetAsync(resourceGroupName, automationAccountName, runbookName);

                return automationRunbookResult.Runbook.Properties.Description;
            }
        }

        private async Task<IEnumerable<Models.Runbook>> ListAutomationRunbooks(string accessToken, string subscriptionId, string resourceGroupName, string automationAccountName, params string[] runbooksStateFilter)
        {
            var credentials = new TokenCloudCredentials(subscriptionId, accessToken);

            using (var automationClient = new AutomationManagementClient(credentials))
            {
                var automationRunbooksResult = await automationClient.Runbooks.ListAsync(resourceGroupName, automationAccountName);

                var automationRunbooks = await Task.WhenAll(automationRunbooksResult.Runbooks.Where(x => (runbooksStateFilter == null || !runbooksStateFilter.Any()) || runbooksStateFilter.Contains(x.Properties.State, StringComparer.InvariantCultureIgnoreCase))
                    .Select(async runbook => new Models.Runbook
                    {
                        RunbookId = runbook.Id,
                        RunbookName = runbook.Name,
                        RunbookState = runbook.Properties.State,
                        RunbookParameters = await this.ListAutomationRunbookParameters(accessToken, subscriptionId, resourceGroupName, automationAccountName, runbook.Name)
                    }).ToList());

                return automationRunbooks;
            }
        }

        private async Task<IEnumerable<RunbookJob>> ListAutomationJobs(string accessToken, string subscriptionId, string resourceGroupName, string automationAccountName)
        {
            var credentials = new TokenCloudCredentials(subscriptionId, accessToken);

            using (var automationClient = new AutomationManagementClient(credentials))
            {
                var automationJobsResult = await automationClient.Jobs.ListAsync(resourceGroupName, automationAccountName, parameters: null);

                var automationJobs = automationJobsResult.Jobs.Select(
                     job => new RunbookJob
                     {
                         JobId = job.Properties.JobId.ToString(),
                         Status = job.Properties.Status,
                         RunbookName = job.Properties.Runbook?.Name ?? "_(Unknown)_"
                     }).ToList();

                return automationJobs;
            }
        }

        private async Task<IEnumerable<Models.RunbookParameter>> ListAutomationRunbookParameters(
            string accessToken,
            string subscriptionId,
            string resourceGroupName,
            string automationAccountName,
            string runbookName)
        {
            var credentials = new TokenCloudCredentials(subscriptionId, accessToken);

            using (var automationClient = new AutomationManagementClient(credentials))
            {
                var automationRunbookResult = await automationClient.Runbooks.GetAsync(resourceGroupName, automationAccountName, runbookName);

                var automationRunbookPrameters = automationRunbookResult.Runbook.Properties.Parameters.Select(
                    parameter => new Models.RunbookParameter
                    {
                        ParameterName = parameter.Key,
                        DefaultValue = parameter.Value.DefaultValue,
                        IsMandatory = parameter.Value.IsMandatory,
                        Position = parameter.Value.Position,
                        Type = parameter.Value.Type
                    }).ToList();

                return automationRunbookPrameters;
            }
        }

    }
}
