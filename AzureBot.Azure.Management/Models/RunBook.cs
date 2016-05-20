namespace AzureBot.Azure.Management.Models
{
    using System;
    using System.Collections.Generic;

    [Serializable]
    public class Runbook
    {
        public string RunbookName { get; set; }

        public string RunbookId { get; set; }

        public IEnumerable<RunbookParameter> RunbookParameters { get; set; }
    }
}
