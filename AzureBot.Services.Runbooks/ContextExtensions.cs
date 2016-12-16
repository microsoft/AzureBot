namespace AzureBot
{
    using System.Collections.Generic;
    using Forms;
    using Microsoft.Bot.Builder.Dialogs;
    using Models;
    public static class ContextExtensions
    {
        public static IList<RunbookJob> GetAutomationJobs(this IBotContext context, string subscriptionId)
        {
            IDictionary<string, IList<RunbookJob>> automationJobsBySubscription;
            IList<RunbookJob> automationJobs;

            if (context.UserData.TryGetValue(AutomationContextConstants.AutomationJobsKey, out automationJobsBySubscription))
            {
                if (automationJobsBySubscription.TryGetValue(subscriptionId, out automationJobs))
                {
                    return automationJobs;
                }
            }

            return null;
        }

        public static void StoreAutomationJobs(this IBotContext context, string subscriptionId, IList<RunbookJob> automationJobs)
        {
            IDictionary<string, IList<RunbookJob>> automationJobsBySubscription;

            if (!context.UserData.TryGetValue(AutomationContextConstants.AutomationJobsKey, out automationJobsBySubscription))
            {
                automationJobsBySubscription = new Dictionary<string, IList<RunbookJob>>();
            }

            automationJobsBySubscription[subscriptionId] = automationJobs;

            context.UserData.SetValue(AutomationContextConstants.AutomationJobsKey, automationJobsBySubscription);
        }

        public static RunbookFormState GetRunbookFormState(this IBotContext context)
        {
            RunbookFormState runbookFormState;

            context.PrivateConversationData.TryGetValue(AutomationContextConstants.RunbookFormStateKey, out runbookFormState);

            return runbookFormState;
        }

        public static void StoreRunbookFormState(this IBotContext context, RunbookFormState runbookFormState)
        {
            context.PrivateConversationData.SetValue(AutomationContextConstants.RunbookFormStateKey, runbookFormState);
        }

        public static void CleanupRunbookFormState(this IBotContext context)
        {
            context.PrivateConversationData.RemoveValue(AutomationContextConstants.RunbookFormStateKey);
        }
    }
}
