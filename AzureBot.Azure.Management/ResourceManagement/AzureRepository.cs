namespace AzureBot.Azure.Management.ResourceManagement
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Data;
    using Microsoft.Azure.Management.Compute;
    using Microsoft.Azure.Subscriptions;
    using Models;
    using TokenCredentials = Microsoft.Azure.TokenCloudCredentials;

    public class AzureRepository
    {
        public async Task<IEnumerable<Subscription>> ListSubscriptionsAsync(string accessToken)
        {
            var credentials = new TokenCredentials(accessToken);

            IEnumerable<Subscription> subscriptions = new List<Subscription>();

            using (SubscriptionClient client = new SubscriptionClient(credentials))
            {
                var result = await client.Subscriptions.ListAsync();

                subscriptions = result.Subscriptions.Select(sub => new Subscription { SubscriptionId= sub.SubscriptionId, DisplayName= sub.DisplayName }).ToList();
            }

            return subscriptions;
        }

        public async Task<IEnumerable<VirtualMachine>> ListVirtualMachinesAsync(string accessToken, string subscriptionId)
        {
            var credentials = new TokenCredentials(subscriptionId, accessToken);
            using (var client = new ComputeManagementClient(credentials))
            {
                var vmList = await client.VirtualMachines.ListAllAsync(null);
                var all = vmList.VirtualMachines.Select(async (vm) =>
                {
                    var segments = vm.Id.Split('/');
                    var resourceGroupName = segments.SkipWhile(segment => segment != "resourceGroups").ElementAtOrDefault(1);
                    var vmName = segments.Last();
                    var response = await client.VirtualMachines.GetWithInstanceViewAsync(resourceGroupName, vmName);
                    var vmInfo = response.VirtualMachine;
                    var vmStatus = vmInfo.InstanceView.Statuses.Where(p => p.Code.StartsWith("PowerState/")).FirstOrDefault();
                    return new VirtualMachine {
                        SubscriptionId = subscriptionId,
                        ResourceGroup = resourceGroupName,
                        Name = vmInfo.Name,
                        Status = vmStatus?.DisplayStatus ?? "NA"
                    };
                });

                return await Task.WhenAll(all.ToArray());
            }
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
