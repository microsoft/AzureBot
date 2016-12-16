namespace AzureBot.Models
{
    using System;

    [Serializable]
    public class VirtualMachine
    {
        public string SubscriptionId { get; set; }

        public string ResourceGroup { get; set; }

        public string Name { get; set; }

        public VirtualMachinePowerState PowerState { get; set; }

        public string Size { get; set; }

        public override string ToString()
        {
            return $"{Name} (Status: {PowerState}, ResourceGroup: {ResourceGroup}, Size: {Size})";
        }
    }
}