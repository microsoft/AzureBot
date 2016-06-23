namespace AzureBot.Forms
{
    using System;
    using Microsoft.Bot.Builder.FormFlow;
    [Serializable]
    public class RunbookParameterFormState
    {
        public RunbookParameterFormState(bool isMandatory)
        {
            this.IsMandatory = isMandatory;
        }

        public bool IsMandatory { get; private set; }

        public string ParameterName { get; set; }

        public string ParameterValue { get; set; }
    }
}