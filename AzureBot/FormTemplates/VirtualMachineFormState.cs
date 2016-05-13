namespace AzureBot.FormTemplates
{
    using System;
    using System.Collections.Generic;
    using Azure.Management.Models;
    using Microsoft.Bot.Builder.FormFlow;
    [Serializable]
    public class VirtualMachineFormState
    {
        public VirtualMachineFormState(IEnumerable<string> availableVMs)
        {
            this.AvailableVMs = availableVMs;
        }

        public string VirtualMachine { get; set; }

        public IEnumerable<string> AvailableVMs { get; private set; }
    }
}