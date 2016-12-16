namespace AzureBot.Models
{
    using System;

    [Serializable]
    public class RunbookJob
    {
        public string ResourceGroupName { get; set; }

        public string AutomationAccountName { get; set; }

        public string RunbookName { get; set; }

        public string JobId { get; set; }

        public string Status { get; set; }

        public DateTimeOffset? StartDateTime { get; set; }

        public DateTimeOffset? EndDateTime { get; set; }

        public string FriendlyJobId { get; set; }
    }
}
