namespace AzureBot.Forms
{
    using Models;
    using System;
    using System.Collections.Generic;

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