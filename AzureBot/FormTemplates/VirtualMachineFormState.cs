namespace AzureBot.FormTemplates
{
    using System;
    using System.Collections.Generic;

    [Serializable]
    public class VirtualMachineFormState
    {
        public VirtualMachineFormState(IEnumerable<string> availableVMs, Operations operation)
        {
            this.AvailableVMs = availableVMs;
            this.Operation = operation;
        }

        public string VirtualMachine { get; set; }

        public IEnumerable<string> AvailableVMs { get; private set; }

        public Operations Operation { get; private set; }
    }
}