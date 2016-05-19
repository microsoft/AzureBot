namespace AzureBot.Azure.Management.Models
{
    using System;

    [Serializable]
    public class RunbookParameter
    {
        public string ParameterName { get; set; }

        public string DefaultValue { get; set; }

        public bool IsMandatory { get; set; }

        public int Position { get; set; }

        public string Type { get; set; }
    }
}
