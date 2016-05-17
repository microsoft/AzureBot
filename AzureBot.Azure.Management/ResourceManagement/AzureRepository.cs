namespace AzureBot.Azure.Management.ResourceManagement
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Data;
    using Microsoft.Azure.Management.Compute;
    using Microsoft.Azure.Management.ResourceManager;
    using Microsoft.Rest;
    using Models;

    public class AzureRepository
    {
        private string accessToken;

        public AzureRepository(string accessToken)
        {
            this.accessToken = accessToken;
        }

        public async Task<IEnumerable<Subscription>> ListSubscriptionsAsync()
        {
            return await IsolatedService.Marshal(async (args) =>
            {
                var credentials = new TokenCredentials((string)args[0]);
                using (var client = new SubscriptionClient(credentials))
                {
                    var subscriptions = await client.Subscriptions.ListAsync();
                    return subscriptions.Select(p => new Subscription { DisplayName = p.DisplayName, SubscriptionId = p.SubscriptionId }).ToArray();
                }
            }, this.accessToken);
        }

        public async Task<IEnumerable<VirtualMachine>> ListVirtualMachinesAsync(string subscriptionId)
        {
            return await IsolatedService.Marshal(async (args) =>
            {
                var credentials = new TokenCredentials((string)args[0]);
                using (var client = new ComputeManagementClient(credentials))
                {
                    client.SubscriptionId = (string)args[1];
                    var virtualMachines = await client.VirtualMachines.ListAllAsync();
                    return virtualMachines.Select(p => new VirtualMachine { SubscriptionId = p.Id, Name = p.Name }).ToArray();
                }
            }, this.accessToken, subscriptionId);
        }

        public async Task<IEnumerable<AutomationAccount>> ListAutomationAccountsAsync(string subscriptionId)
        {
            return await Task.FromResult(MockData.GetAutomationAccounts());
        }

        public async Task<bool> StartVirtualMachineAsync(string subscriptionId, string virtualMachineName)
        {
            return await Task.FromResult(true);
        }

        public async Task<bool> StopVirtualMachineAsync(string subscriptionId, string virtualMachineName)
        {
            return await Task.FromResult(true);
        }

        public async Task<bool> RunRunBookAsync(string subscriptionId, string automationAccountName, string runBookName)
        {
            return await Task.FromResult(true);
        }
    }
}
