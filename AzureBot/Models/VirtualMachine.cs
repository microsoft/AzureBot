namespace AzureBot.Models
{
    using System;
    using Microsoft.Bot.Builder.FormFlow;

    [Serializable]
    public class VirtualMachine
    {
        public ServiceAction Action { get; set; }

        public string VirtualMachineName { get; set; }

        public static IForm<VirtualMachine> BuildForm()
        {
            return new FormBuilder<VirtualMachine>()
                    .Message("Welcome to the Virtual Machine dialog!")
                    .Build();
        }
    }
}