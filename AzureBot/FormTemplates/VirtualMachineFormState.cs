namespace AzureBot.FormTemplates
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Azure.Management.Models;

    [Serializable]
    public class VirtualMachineFormState
    {
        public VirtualMachineFormState(IEnumerable<VirtualMachine> availableVMs, Operations operation)
        {
            this.AvailableVMs = availableVMs;
            this.Operation = operation;
        }

        public string VirtualMachine { get; set; }

        public IEnumerable<VirtualMachine> AvailableVMs { get; private set; }

        public Operations Operation { get; private set; }

        public VirtualMachine SelectedVM
        {
            get
            {
                return this.AvailableVMs.Where(p => p.Name == this.VirtualMachine).SingleOrDefault();
            }
        }
    }
}