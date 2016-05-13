namespace AzureBot.Azure.Management.ResourceManagement
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Data;
    using Models;

    public class AzureRepository
    {
        public async Task<IEnumerable<Subscription>> ListSubscriptionsAsync()
        {
            return await Task.FromResult(MockData.GetSubscriptions());
        }

        public async Task<IEnumerable<VirtualMachine>> ListVirtualMachinesAsync(string subscriptionId)
        {
            return await Task.FromResult(MockData.GetVirtualMachines().Where(p => p.SubscriptionId == subscriptionId));
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
