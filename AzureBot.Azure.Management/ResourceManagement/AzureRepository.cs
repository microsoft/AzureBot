namespace AzureBot.Azure.Management.ResourceManagement
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Models;
    using Data;

    public class AzureRepository
    {
        public async Task<IEnumerable<Subscription>> ListSubscriptionsAsync()
        {
            return await Task.FromResult(MockData.GetSubscriptions());
        }

        public async Task<IEnumerable<VirtualMachine>> ListVirtualMachinesAsync(string subscriptionId)
        {
            return await Task.FromResult(MockData.GetVirtualMachines());
        }

        public async Task<bool> StartVirtualMachineAsync(string subscriptionId, string virtualMachineName)
        {
            return await Task.FromResult(true);
        }
        public async Task<bool> StopVirtualMachineAsync(string subscriptionId, string virtualMachineName)
        {
            return await Task.FromResult(true);
        }
    }
}
