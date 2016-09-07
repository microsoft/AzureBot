using AzureBot.Forms;
using Microsoft.Bot.Builder.FormFlow;
using Microsoft.Bot.Builder.FormFlow.Advanced;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureBot.Services.VMs.Forms
{
    class VMForms
    {

        public static IForm<VirtualMachineFormState> BuildVirtualMachinesForm()
        {
            return EntityForms.CreateCustomForm<VirtualMachineFormState>()
                .Field(nameof(VirtualMachineFormState.Operation), (state) => false)
                .Field(new FieldReflector<VirtualMachineFormState>(nameof(VirtualMachineFormState.VirtualMachine))
                .SetType(null)
                .SetPrompt(EntityForms.PerLinePromptAttribute("Please select the virtual machine you want to {Operation}: {||}"))
                .SetDefine((state, field) =>
                {
                    foreach (var vm in state.AvailableVMs)
                    {
                        field
                            .AddDescription(vm.Name, vm.ToString())
                            .AddTerms(vm.Name, vm.Name);
                    }

                    return Task.FromResult(true);
                }))
               .Confirm("Would you like to {Operation} virtual machine '{VirtualMachine}'?", (state) => (Operations)Enum.Parse(typeof(Operations), state.Operation, true) == Operations.Start, null)
               .Confirm("Would you like to {Operation} virtual machine '{VirtualMachine}'? Please note that your VM will still incur compute charges.", (state) => (Operations)Enum.Parse(typeof(Operations), state.Operation, true) == Operations.Shutdown, null)
               .Confirm("Would you like to {Operation} virtual machine '{VirtualMachine}'? Your VM won't incur charges and all IP addresses will be released.", (state) => (Operations)Enum.Parse(typeof(Operations), state.Operation, true) == Operations.Stop, null)
               .Build();
        }

        public static IForm<AllVirtualMachinesFormState> BuildAllVirtualMachinesForm()
        {
            return EntityForms.CreateCustomForm<AllVirtualMachinesFormState>()
                .Field(nameof(AllVirtualMachinesFormState.Operation), (state) => false)
                .Field(nameof(AllVirtualMachinesFormState.VirtualMachines), (state) => false)
               .Confirm("You are trying to {Operation} the following virtual machines: {VirtualMachines} Are you sure?", (state) => (Operations)Enum.Parse(typeof(Operations), state.Operation, true) == Operations.Start, null)
               .Confirm("You are trying to {Operation} the following virtual machines: {VirtualMachines} Are you sure? Please note that your VMs will still incur compute charges.", (state) => (Operations)Enum.Parse(typeof(Operations), state.Operation, true) == Operations.Shutdown, null)
               .Confirm("You are trying to {Operation} the following virtual machines: {VirtualMachines} Are you sure? Your VMs won't incur charges and all IP addresses will be released.", (state) => (Operations)Enum.Parse(typeof(Operations), state.Operation, true) == Operations.Stop, null)
               .Build();
        }

    }
}
