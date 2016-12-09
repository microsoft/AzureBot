namespace AzureBot.Forms
{
    using Models;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    [Serializable]
    public class RunbookFormState
    {
        public RunbookFormState(IEnumerable<AutomationAccount> availableAutomationAccounts)
        {
            this.AvailableAutomationAccounts = availableAutomationAccounts;
            this.RunbookParameters = new List<RunbookParameterFormState>();
        }

        public string AutomationAccountName { get; set; }

        public string RunbookName { get; set; }

        public IList<RunbookParameterFormState> RunbookParameters { get; set; }

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