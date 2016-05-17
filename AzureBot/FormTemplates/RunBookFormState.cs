namespace AzureBot.FormTemplates
{
    using System;
    using System.Collections.Generic;
    using Azure.Management.Models;
    [Serializable]
    public class RunBookFormState
    {
        public RunBookFormState(IEnumerable<AutomationAccount> availableAutomationAccounts)
        {
            this.AvailableAutomationAccounts = availableAutomationAccounts;
        }

        public string AutomationAccountName { get; set; }

        public string RunBookName { get; set; }

        public IEnumerable<AutomationAccount> AvailableAutomationAccounts { get; private set; }
    }
}