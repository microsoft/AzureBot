namespace AzureBot.Azure.Management.Models
{
    using System;
    using System.Collections.Generic;

    [Serializable]
    public class AutomationAccount
    {
        public string AutomationAccountName { get; set; }

        public string AutomationAccountId { get; set; }

        public IEnumerable<RunBook> RunBooks { get; set; }
    }
}
