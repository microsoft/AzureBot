namespace AzureBot.Azure.Management.ResourceManagement
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Azure.Management.Automation;
    using Microsoft.Azure.Management.Compute;
    using Microsoft.Azure.Subscriptions;
    using Models;
    using AzureModels = Microsoft.Azure.Management.Automation.Models;
    using TokenCredentials = Microsoft.Azure.TokenCloudCredentials;

    public class AzureRepository
    {
        public async Task<IEnumerable<Subscription>> ListSubscriptionsAsync(string accessToken)
        {
            var credentials = new TokenCredentials(accessToken);

            using (SubscriptionClient client = new SubscriptionClient(credentials))
            {
                var subscriptionsResult = await client.Subscriptions.ListAsync().ConfigureAwait(false);
                var subscriptions = subscriptionsResult.Subscriptions.Select(sub => new Subscription { SubscriptionId = sub.SubscriptionId, DisplayName = sub.DisplayName }).ToList();
                return subscriptions;
            }
        }

        public async Task<IEnumerable<VirtualMachine>> ListVirtualMachinesAsync(string accessToken, string subscriptionId)
        {
            var credentials = new TokenCredentials(subscriptionId, accessToken);
            using (var client = new ComputeManagementClient(credentials))
            {
                var virtualMachinesResult = await client.VirtualMachines.ListAllAsync(null).ConfigureAwait(false);
                var all = virtualMachinesResult.VirtualMachines.Select(async (vm) =>
                {
                    var resourceGroupName = GetResourceGroup(vm.Id);
                    var response = await client.VirtualMachines.GetWithInstanceViewAsync(resourceGroupName, vm.Name);
                    var vmStatus = response.VirtualMachine.InstanceView.Statuses.Where(p => p.Code.ToLower().StartsWith("powerstate/")).FirstOrDefault();
                    return new VirtualMachine
                    {
                        SubscriptionId = subscriptionId,
                        ResourceGroup = resourceGroupName,
                        Name = vm.Name,
                        PowerState = GetVirtualMachinePowerState(vmStatus?.Code.ToLower() ?? "na")
                    };
                });

                return await Task.WhenAll(all.ToArray());
            }
        }

        public async Task<IEnumerable<AutomationAccount>> ListAutomationAccountsAsync(string accessToken, string subscriptionId)
        {
            var credentials = new TokenCredentials(subscriptionId, accessToken);

            using (var automationClient = new AutomationManagementClient(credentials))
            {
                var automationAccountsResult = await automationClient.AutomationAccounts.ListAsync(null).ConfigureAwait(false);
                var automationAccounts = await Task.WhenAll(
                    automationAccountsResult.AutomationAccounts.Select(
                        async account => new AutomationAccount
                        {
                            SubscriptionId = subscriptionId,
                            ResourceGroup = GetResourceGroup(account.Id),
                            AutomationAccountId = account.Id,
                            AutomationAccountName = account.Name,
                            Runbooks = await this.ListAutomationRunbooks(accessToken, subscriptionId, GetResourceGroup(account.Id), account.Name)
                        }).ToList());
                return automationAccounts;
            }
        }

        public async Task<IEnumerable<Runbook>> ListAutomationRunbooks(string accessToken, string subscriptionId, string resourceGroupName, string automationAccountName)
        {
            var credentials = new TokenCredentials(subscriptionId, accessToken);

            using (var automationClient = new AutomationManagementClient(credentials))
            {
                var automationRunbooksResult = await automationClient.Runbooks.ListAsync(resourceGroupName, automationAccountName);

                var automationRunbooks = await Task.WhenAll(automationRunbooksResult.Runbooks.Select(
                    async runbook => new Runbook
                    {
                        RunbookId = runbook.Id,
                        RunbookName = runbook.Name,
                        RunbookParameters = await this.ListAutomationRunbookParameters(accessToken, subscriptionId, resourceGroupName, automationAccountName, runbook.Name)
                    }).ToList());

                return automationRunbooks;
            }
        }

        public async Task<IEnumerable<RunbookParameter>> ListAutomationRunbookParameters(
            string accessToken, 
            string subscriptionId, 
            string resourceGroupName, 
            string automationAccountName, 
            string runbookName)
        {
            var credentials = new TokenCredentials(subscriptionId, accessToken);

            using (var automationClient = new AutomationManagementClient(credentials))
            {
                var automationRunbookResult = await automationClient.Runbooks.GetAsync(resourceGroupName, automationAccountName, runbookName);

                var automationRunbookPrameters = automationRunbookResult.Runbook.Properties.Parameters.Select(
                    parameter => new RunbookParameter
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

        public async Task<bool> StartVirtualMachineAsync(string accessToken, string subscriptionId, string resourceGroupName, string virtualMachineName)
        {
            var credentials = new TokenCredentials(subscriptionId, accessToken);
            using (var client = new ComputeManagementClient(credentials))
            {
                var status = await client.VirtualMachines.StartAsync(resourceGroupName, virtualMachineName).ConfigureAwait(false);
                return status.Status != Microsoft.Azure.Management.Compute.Models.ComputeOperationStatus.Failed;
            }
        }

        public async Task<bool> StopVirtualMachineAsync(string accessToken, string subscriptionId, string resourceGroupName, string virtualMachineName)
        {
            var credentials = new TokenCredentials(subscriptionId, accessToken);
            using (var client = new ComputeManagementClient(credentials))
            {
                var status = await client.VirtualMachines.PowerOffAsync(resourceGroupName, virtualMachineName).ConfigureAwait(false);
                return status.Status != Microsoft.Azure.Management.Compute.Models.ComputeOperationStatus.Failed;
            }
        }

        public async Task<bool> StartRunbookAsync(string accessToken, string subscriptionId, string resourceGroupName, string automationAccountName, string runbookName)
        {
            var credentials = new TokenCredentials(subscriptionId, accessToken);

            using (var client = new AutomationManagementClient(credentials))
            {
                var parameters = new AzureModels.JobCreateParameters(
                    new AzureModels.JobCreateProperties(
                        new AzureModels.RunbookAssociationProperty
                        {
                            Name = runbookName
                        }));
                var jobCreateResult = await client.Jobs.CreateAsync(resourceGroupName, automationAccountName, parameters).ConfigureAwait(false);
                return jobCreateResult.StatusCode == System.Net.HttpStatusCode.Created;
            }
        }

        private static string GetResourceGroup(string id)
        {
            var segments = id.Split('/');
            var resourceGroupName = segments.SkipWhile(segment => segment != "resourceGroups").ElementAtOrDefault(1);
            return resourceGroupName;
        }

        private VirtualMachinePowerState GetVirtualMachinePowerState(string code)
        {
            if (code.EndsWith("/running"))
            {
                return VirtualMachinePowerState.Running;
            }
            else if (code.EndsWith("/stopped"))
            {
                return VirtualMachinePowerState.Stopped;
            }
            else
            {
                return VirtualMachinePowerState.Unknown;
            }
        }
    }
}