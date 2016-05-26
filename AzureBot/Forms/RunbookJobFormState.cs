namespace AzureBot.Forms
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AzureBot.Azure.Management.Models;

    [Serializable]
    public class RunbookJobFormState
    {
        public RunbookJobFormState(IEnumerable<AutomationAccount> availableAutomationAccounts)
        {
            this.AvailableAutomationAccounts = availableAutomationAccounts;
        }

        public string AutomationAccountName { get; set; }

        public string RunbookJobId { get; set; }

        public IEnumerable<AutomationAccount> AvailableAutomationAccounts { get; private set; }

        public AutomationAccount SelectedAutomationAccount
        {
            get
            {
                return this.AvailableAutomationAccounts.SingleOrDefault(account => account.AutomationAccountName == this.AutomationAccountName);
            }
        }

        public RunbookJob SelectedRunbookJob
        {
            get
            {
                return this.SelectedAutomationAccount.RunbookJobs.SingleOrDefault(runbookJob => runbookJob.JobId == this.RunbookJobId);
            }
        }
    }
}