namespace AzureBot.Forms
{
    using System;
    
    [Serializable]
    public class RunbookParameterFormState
    {
        public RunbookParameterFormState(bool isMandatory, bool isFirstParameter, string runbookName)
        {
            this.IsMandatory = isMandatory;
            this.IsFirstParameter = isFirstParameter;
            this.RunbookName = runbookName;
        }

        public bool IsFirstParameter { get; private set; }

        public bool IsMandatory { get; private set; }

        public string ParameterName { get; set; }

        public string ParameterValue { get; set; }

        public string RunbookName { get; private set; }
    }
}