namespace AzureBot.FormTemplates
{
    using System;
    using System.Collections.Generic;
    using Azure.Management.Models;

    [Serializable]
    public class SubscriptionFormState
    {
        public SubscriptionFormState(IEnumerable<Subscription> availableSubscriptions)
        {
            this.AvailableSubscriptions = availableSubscriptions;
        }

        public string DisplayName { get; set; }

        public string SubscriptionId { get; set; }

        public IEnumerable<Subscription> AvailableSubscriptions { get; private set; }
    }
}