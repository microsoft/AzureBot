namespace AzureBot.Azure.Management.Models
{
    using System;

    [Serializable]
    public class VirtualMachine
    {
        public string SubscriptionId { get; set; }

        public string ResourceGroup { get; set; }

        public string Name { get; set; }

        public VirtualMachinePowerState PowerState { get; set; }
    }
}