namespace AzureBot.Models
{
    using System;
    using System.Collections.Generic;

    [Serializable]
    public class Runbook
    {
        public string RunbookName { get; set; }

        public string RunbookId { get; set; }

        public string RunbookState { get; set; }

        public IEnumerable<RunbookParameter> RunbookParameters { get; set; }

        public override string ToString()
        {
            return $"{this.RunbookName} (State: {this.RunbookState})";
        }
    }
}
