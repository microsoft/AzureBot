namespace AzureBot.FormTemplates
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Azure.Management.Models;

    [Serializable]
    public class RunbookFormState
    {
        public RunbookFormState(IEnumerable<AutomationAccount> availableAutomationAccounts)
        {
            this.AvailableAutomationAccounts = availableAutomationAccounts;
        }

        public string AutomationAccountName { get; set; }

        public string RunbookName { get; set; }

        public IEnumerable<RunbookParameter> AvailableRunbookParameters { get; set; }

        public IEnumerable<AutomationAccount> AvailableAutomationAccounts { get; private set; }

        public AutomationAccount SelectedAutomationAccount
        {
            get
            {
                return this.AvailableAutomationAccounts.SingleOrDefault(account => account.AutomationAccountName == this.AutomationAccountName);
            }
        }

        public Runbook SelectedRunbook
        {
            get
            {
                return this.SelectedAutomationAccount.Runbooks.SingleOrDefault(runbook => runbook.RunbookName == this.RunbookName);
            }
        }
    }
}