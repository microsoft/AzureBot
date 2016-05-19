namespace AzureBot
{
    using System.Linq;
    using System.Threading.Tasks;
    using FormTemplates;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.FormFlow;
    using Microsoft.Bot.Builder.FormFlow.Advanced;

    public class EntityForms
    {
        public static IForm<SubscriptionFormState> BuildSubscriptionForm()
        {
            var prompt = new PromptAttribute()
            {
                ChoiceStyle = ChoiceStyleOptions.PerLine
            };
            return new FormBuilder<SubscriptionFormState>()
                .Field(new FieldReflector<SubscriptionFormState>(nameof(SubscriptionFormState.SubscriptionId))
                .SetType(null)
                .SetPrompt(PerLinePromptAttribute("Please select the subscription you want to work with: {||}"))
                .SetDefine((state, field) =>
                {
                    foreach (var sub in state.AvailableSubscriptions)
                    {
                        field.AddDescription(sub.SubscriptionId, sub.DisplayName)
                            .AddTerms(sub.SubscriptionId, sub.DisplayName);
                    }

                    return Task.FromResult(true);
                }))
               .Build();
        }

        public static IForm<VirtualMachineFormState> BuildVirtualMachinesForm()
        {
            return new FormBuilder<VirtualMachineFormState>()
                .Field(nameof(VirtualMachineFormState.Operation), (state) => false)
                .Field(new FieldReflector<VirtualMachineFormState>(nameof(VirtualMachineFormState.VirtualMachine))
                .SetType(null)
                .SetPrompt(PerLinePromptAttribute("Please select the virtual machine you want to {Operation}: {||}"))
                .SetDefine((state, field) =>
                {
                    foreach (var vm in state.AvailableVMs)
                    {
                        field
                            .AddDescription(vm.Name, vm.Name)
                            .AddTerms(vm.Name, vm.Name);
                    }

                    return Task.FromResult(true);
                }))
               .Confirm("Would you like to {Operation} virtual machine '{VirtualMachine}'?")
               .Build();
        }

        public static IForm<RunbookFormState> BuildRunbookForm()
        {
            return new FormBuilder<RunbookFormState>()
                .Field(new FieldReflector<RunbookFormState>(nameof(RunbookFormState.AutomationAccountName))
                    .SetType(null)
                    .SetPrompt(PerLinePromptAttribute("Please select the automation account you want to use: {||}"))
                    .SetDefine((state, field) =>
                    {
                        foreach (var account in state.AvailableAutomationAccounts)
                        {
                            field
                                .AddDescription(account.AutomationAccountName, account.AutomationAccountName)
                                .AddTerms(account.AutomationAccountName, account.AutomationAccountName);
                        }

                        return Task.FromResult(true);
                    }))
                .Field(new FieldReflector<RunbookFormState>(nameof(RunbookFormState.RunbookName))
                    .SetType(null)
                    .SetPrompt(PerLinePromptAttribute("Please select the runbook you want to run: {||}"))
                    .SetActive(state => !string.IsNullOrWhiteSpace(state.AutomationAccountName))
                    .SetDefine((state, field) =>
                    {
                        if (string.IsNullOrWhiteSpace(state.AutomationAccountName))
                        {
                            return Task.FromResult(false);
                        }

                        foreach (var runbook in state.AvailableAutomationAccounts.Single(
                            a => a.AutomationAccountName == state.AutomationAccountName).Runbooks)
                        {
                            field
                                .AddDescription(runbook.RunbookName, runbook.RunbookName)
                                .AddTerms(runbook.RunbookName, runbook.RunbookName);
                        }

                        return Task.FromResult(true);
                    }))
               .Confirm("Would you like to run runbook '{RunbookName}'?")
               .Build();
        }

        public static IForm<RunbookFormState> BuildRunbookParametersForm(RunbookFormState state)
        {
            var paramatersFormBuilder = new FormBuilder<RunbookFormState>()
                .Confirm("The Runbook you selected has parameters. Would you like to enter their values?");

            foreach (var parameter in state.SelectedRunbook.RunbookParameters)
            {
                paramatersFormBuilder.Field(new FieldReflector<RunbookFormState>(parameter.ParameterName)
                    .SetPrompt(new PromptAttribute("Please enter the value for parameter: " + parameter.ParameterName)));
            }

            return paramatersFormBuilder.Build();
        }

        private static PromptAttribute PerLinePromptAttribute(string pattern)
        {
            return new PromptAttribute(pattern)
            {
                ChoiceStyle = ChoiceStyleOptions.PerLine
            };
        }
    }
}