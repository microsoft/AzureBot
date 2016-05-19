namespace AzureBot.Azure.Management.Models
{
    using System;
    using System.Collections.Generic;

    [Serializable]
    public class AutomationAccount
    {
        public string SubscriptionId { get; set; }

        public string ResourceGroup { get; set; }

        public string AutomationAccountName { get; set; }

        public string AutomationAccountId { get; set; }

        public IEnumerable<Runbook> Runbooks { get; set; }
    }
}
