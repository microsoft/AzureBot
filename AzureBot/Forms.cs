using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using AzureBot.Azure.Management.ResourceManagement;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.FormFlow;
using Microsoft.Bot.Builder.FormFlow.Advanced;
using AzureBot.FormTemplates;

namespace AzureBot
{
    public class Forms
    {
        public static IForm<SubscriptionFormState> BuildSubscriptionForm()
        {
            return new FormBuilder<SubscriptionFormState>()
                .Message("Select the subscription you want to work with")
                .Field(new FieldReflector<SubscriptionFormState>(nameof(SubscriptionFormState.SubscriptionId))
                .SetType(null)
                .SetDefine(async (state, field) =>
               {
                   var subscriptions = await (new AzureRepository().ListSubscriptionsAsync());
                   foreach (var sub in subscriptions)
                   {
                       field.AddDescription(sub.SubscriptionId, sub.DisplayName)
                           .AddTerms(sub.SubscriptionId, sub.DisplayName);
                   }

                   return true;
               }))
               .Build();
        }

        public static IForm<VirtualMachineFormState> BuildVirtualMachinesForm()
        {
            return new FormBuilder<VirtualMachineFormState>()
                .Field(new FieldReflector<VirtualMachineFormState>(nameof(VirtualMachineFormState.Name))
                .SetType(null)
                .SetDefine(async (state, field) =>
                {
                    // TODO: need to access the current subscriptionID 
                    var subscriptionId = "NOT IMPLEMENTED";
                    var virtualMachines = await (new AzureRepository().ListVirtualMachinesAsync(subscriptionId));
                    foreach (var vm in virtualMachines)
                    {
                        field
                            .AddDescription(vm.Name, vm.Name)
                            .AddTerms(vm.Name, vm.Name);
                    }

                    return true;
                }))
               .Confirm("Would you like to start virtual machine {Name}?")
               .Build();
        }
    }
}