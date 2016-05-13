namespace AzureBot.FormTemplates
{
    using System;
    using System.Collections.Generic;

    [Serializable]
    public class SubscriptionFormState
    {
        public SubscriptionFormState(IDictionary<string, string> availableSubscriptions)
        {
            this.AvailableSubscriptions = availableSubscriptions;
        }

        public string DisplayName { get; set; }

        public string SubscriptionId { get; set; }

        public IDictionary<string, string> AvailableSubscriptions { get; private set; }
    }
}