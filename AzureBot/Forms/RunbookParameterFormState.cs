namespace AzureBot.Forms
{
    using System;

    [Serializable]
    public class RunbookParameterFormState
    {
        public string ParameterName { get; set; }

        public string ParameterValue { get; set; }
    }
}