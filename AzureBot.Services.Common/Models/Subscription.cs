namespace AzureBot.Models
{
    using System;

    [Serializable]
    public class Subscription
    {
        public string DisplayName { get; internal set; }

        public string SubscriptionId { get; internal set; }
    }
}