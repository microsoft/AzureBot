namespace AzureBot.Forms
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Models;
    [Serializable]
    public class AllVirtualMachinesFormState
    {
        public AllVirtualMachinesFormState(IEnumerable<VirtualMachine> availableVMs, Operations operation)
        {
            this.AvailableVMs = availableVMs;
            this.Operation = operation.ToString().ToLower();
        }

        public IEnumerable<VirtualMachine> AvailableVMs { get; private set; }

        public string Operation { get; private set; }

        public string VirtualMachines
        {
            get
            {
                return "\n\r" + this.AvailableVMs.Aggregate(
                     string.Empty,
                    (current, next) =>
                    {
                        return current += $"\n\r• {next}";
                    }) + " \n\r";
            }
        }
    }
}