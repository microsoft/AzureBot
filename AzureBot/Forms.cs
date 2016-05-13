namespace AzureBot
{
    using System.Threading.Tasks;
    using AzureBot.Azure.Management.ResourceManagement;
    using AzureBot.FormTemplates;
    using Microsoft.Bot.Builder.FormFlow;
    using Microsoft.Bot.Builder.FormFlow.Advanced;

    public class Forms
    {
        public static IForm<SubscriptionFormState> BuildSubscriptionForm()
        {
            return new FormBuilder<SubscriptionFormState>()
                .Field(new FieldReflector<SubscriptionFormState>(nameof(SubscriptionFormState.SubscriptionId))
                .SetType(null)
                .SetPrompt(new PromptAttribute("Please select the subscription you want to work with: {||}"))
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
                .SetDefine((state, field) =>
                {
                    foreach (var vm in state.AvailableVMs)
                    {
                        field
                            .AddDescription(vm, vm)
                            .AddTerms(vm, vm);
                    }

                    return Task.FromResult(true);
                }))
               .Confirm("Would you like to start virtual machine {Name}?")
               .Build();
        }
    }
}