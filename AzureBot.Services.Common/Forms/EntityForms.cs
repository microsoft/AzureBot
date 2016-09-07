namespace AzureBot.Forms
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
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

            return CreateCustomForm<SubscriptionFormState>()
                .Field(new FieldReflector<SubscriptionFormState>(nameof(SubscriptionFormState.SubscriptionId))
                .SetType(null)
                .SetActive(x => x.AvailableSubscriptions.Any())
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

        public static IFormBuilder<T> CreateCustomForm<T>()
           where T : class
        {
            var form = new FormBuilder<T>();
            var command = form.Configuration.Commands[FormCommand.Quit];
            var terms = command.Terms.ToList();
            terms.Add("cancel");
            command.Terms = terms.ToArray();

            var templateAttribute = form.Configuration.Template(TemplateUsage.NotUnderstood);
            var patterns = templateAttribute.Patterns;
            patterns[0] += " Type *cancel* to quit or *help* if you want more information.";
            templateAttribute.Patterns = patterns;

            return form;
        }

        public static PromptAttribute PerLinePromptAttribute(string pattern)
        {
            return new PromptAttribute(pattern)
            {
                ChoiceStyle = ChoiceStyleOptions.PerLine
            };
        }
    }
}
