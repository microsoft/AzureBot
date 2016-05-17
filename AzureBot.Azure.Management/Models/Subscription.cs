namespace AzureBot.Azure.Management.Models
{
    using System;

    [Serializable]
    public class Subscription
    {
        public Subscription(string subscriptionId, string displayName)
        {
            this.SubscriptionId = subscriptionId;
            this.DisplayName = displayName;
        }

        public string DisplayName { get; private set; }

        public string SubscriptionId { get; private set; }
    }
}