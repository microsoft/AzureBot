namespace AzureBot.Azure.Management.Models
{
    public class VirtualMachine
    {
        public string SubscriptionId { get; set; }

        public string ResourceGroup { get; set; }

        public string Name { get; set; }

        public string Status { get; set; }
    }
}