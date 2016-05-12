using System;

namespace AzureBot.Azure.Management.Models
{
    [Serializable]
    public class Subscription
    {
        public string DisplayName { get; set; }

        public string SubscriptionId { get; set; }
    }
}