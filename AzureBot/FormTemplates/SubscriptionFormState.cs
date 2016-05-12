namespace AzureBot.FormTemplates
{
using System;

    [Serializable]
    public class SubscriptionFormState
    {
        public string DisplayName { get; set; }

        public string SubscriptionId { get; set; }
    }
}