namespace AzureBot.Azure.Management.Models
{
    using System;

    [Serializable]
    public class AutomationAccount
    {
        public string AutomationAccountName { get; set; }

        public string AutomationAccountId { get; set; }

        public RunBook[] RunBooks { get; set; }
    }
}
