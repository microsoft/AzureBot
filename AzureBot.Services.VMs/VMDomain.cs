using AzureBot.Models;
using Microsoft.Azure;
using Microsoft.Azure.Management.Compute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AzureBot.Domain
{
    public class VMDomain
    {
        public async Task<IEnumerable<VirtualMachine>> ListVirtualMachinesAsync(string accessToken, string subscriptionId)
        {
            var credentials = new TokenCloudCredentials(subscriptionId, accessToken);
            using (var client = new ComputeManagementClient(credentials))
            {
                var virtualMachinesResult = await client.VirtualMachines.ListAllAsync(null).ConfigureAwait(false);
                var all = virtualMachinesResult.VirtualMachines.Select(async (vm) =>
                {
                    var resourceGroupName = Subscription.GetResourceGroup(vm.Id);
                    var response = await client.VirtualMachines.GetWithInstanceViewAsync(resourceGroupName, vm.Name);
                    var vmStatus = response.VirtualMachine.InstanceView.Statuses.Where(p => p.Code.ToLower().StartsWith("powerstate/")).FirstOrDefault();
                    return new VirtualMachine
                    {
                        SubscriptionId = subscriptionId,
                        ResourceGroup = resourceGroupName,
                        Name = vm.Name,
                        PowerState = GetVirtualMachinePowerState(vmStatus?.Code.ToLower() ?? VirtualMachinePowerState.Unknown.ToString()),
                        Size = response.VirtualMachine.HardwareProfile.VirtualMachineSize
                    };
                });

                return await Task.WhenAll(all.ToList());
            }
        }

        public async Task<bool> StartVirtualMachineAsync(string accessToken, string subscriptionId, string resourceGroupName, string virtualMachineName)
        {
            var credentials = new TokenCloudCredentials(subscriptionId, accessToken);
            using (var client = new ComputeManagementClient(credentials))
            {
                var status = await client.VirtualMachines.StartAsync(resourceGroupName, virtualMachineName).ConfigureAwait(false);
                return status.Status != Microsoft.Azure.Management.Compute.Models.ComputeOperationStatus.Failed;
            }
        }

        public async Task<bool> PowerOffVirtualMachineAsync(string accessToken, string subscriptionId, string resourceGroupName, string virtualMachineName)
        {
            var credentials = new TokenCloudCredentials(subscriptionId, accessToken);
            using (var client = new ComputeManagementClient(credentials))
            {
                var status = await client.VirtualMachines.PowerOffAsync(resourceGroupName, virtualMachineName).ConfigureAwait(false);
                return status.Status != Microsoft.Azure.Management.Compute.Models.ComputeOperationStatus.Failed;
            }
        }

        public async Task<bool> DeallocateVirtualMachineAsync(string accessToken, string subscriptionId, string resourceGroupName, string virtualMachineName)
        {
            var credentials = new TokenCloudCredentials(subscriptionId, accessToken);
            using (var client = new ComputeManagementClient(credentials))
            {
                var status = await client.VirtualMachines.DeallocateAsync(resourceGroupName, virtualMachineName).ConfigureAwait(false);
                return status.Status != Microsoft.Azure.Management.Compute.Models.ComputeOperationStatus.Failed;
            }
        }

        private VirtualMachinePowerState GetVirtualMachinePowerState(string code)
        {
            string[] powerStateElements = code.Split('/');

            if (powerStateElements.Length != 2)
            {
                return VirtualMachinePowerState.Unknown;
            }

            var status = powerStateElements[1];

            VirtualMachinePowerState powerState;

            if (!Enum.TryParse(status, true, out powerState))
            {
                return VirtualMachinePowerState.Unknown;
            }

            return powerState;
        }
    }
}
