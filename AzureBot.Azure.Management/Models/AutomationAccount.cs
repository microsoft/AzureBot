namespace AzureBot.Azure.Management.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    [Serializable]
    public class AutomationAccount
    {
        public string AutomationAccountName { get; set; }

        public string AutomationAccountId { get; set; }

        public RunBook[] RunBooks { get; set; }
    }
}
