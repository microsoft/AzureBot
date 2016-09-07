using AzureBot.Forms;
using Microsoft.Bot.Builder.FormFlow;
using Microsoft.Bot.Builder.FormFlow.Advanced;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureBot.Services.Runbooks.Forms
{
    class AutomationForms
    {

        public static IForm<RunbookFormState> BuildRunbookForm()
        {
            return EntityForms.CreateCustomForm<RunbookFormState>()
                .Field(new FieldReflector<RunbookFormState>(nameof(RunbookFormState.AutomationAccountName))
                    .SetType(null)
                    .SetPrompt(EntityForms.PerLinePromptAttribute("Please select the automation account you want to use: {||}"))
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
                    .SetPrompt(EntityForms.PerLinePromptAttribute("Please select the runbook you want to run: {||}"))
                    .SetActive(state => !string.IsNullOrWhiteSpace(state.AutomationAccountName))
                    .SetDefine((state, field) =>
                    {
                        if (string.IsNullOrWhiteSpace(state.AutomationAccountName))
                        {
                            return Task.FromResult(false);
                        }

                        foreach (var runbook in state.AvailableAutomationAccounts.Single(
                            a => a.AutomationAccountName == state.AutomationAccountName).Runbooks.Where(x => x.RunbookState.Equals("Published", StringComparison.InvariantCultureIgnoreCase)))
                        {
                            field
                                .AddDescription(runbook.RunbookName, runbook.RunbookName)
                                .AddTerms(runbook.RunbookName, runbook.RunbookName);
                        }

                        return Task.FromResult(true);
                    }))
               .Confirm("Would you like to run runbook '{RunbookName}' of automation acccount '{AutomationAccountName}'?")
               .Build();
        }

        public static IForm<RunbookParameterFormState> BuildRunbookParametersForm()
        {
            return EntityForms.CreateCustomForm<RunbookParameterFormState>()
                .Field(nameof(RunbookParameterFormState.ParameterName), (state) => false)
                .Field(new FieldReflector<RunbookParameterFormState>(nameof(RunbookParameterFormState.ParameterValue))
                    .SetDefine((state, field) =>
                    {
                        var firstParamMessage = state.IsFirstParameter ? $"\n\r If you're unsure what to input, type **quit** followed by **show runbook {state.RunbookName} description** to get more details." : string.Empty;

                        if (!state.IsMandatory)
                        {
                            field.SetOptional(true);

                            field.SetPrompt(new PromptAttribute($"Please enter the value for optional parameter {state.ParameterName} or type *none* to skip it: {firstParamMessage}"));
                        }
                        else
                        {
                            field.SetPrompt(new PromptAttribute($"Please enter the value for mandatory parameter {state.ParameterName}: {firstParamMessage}"));
                        }

                        return Task.FromResult(true);
                    }))
                .Build();
        }

    }
}
