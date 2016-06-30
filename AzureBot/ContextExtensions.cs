namespace AzureBot
{
    using System.Collections.Generic;
    using Azure.Management.Models;
    using Forms;
    using Microsoft.Bot.Builder.Dialogs;

    public static class ContextExtensions
    {
        public static string GetSubscriptionId(this IBotContext context)
        {
            string subscriptionId;

            context.UserData.TryGetValue<string>(ContextConstants.SubscriptionIdKey, out subscriptionId);

            return subscriptionId;
        }

        public static void StoreSubscriptionId(this IBotContext context, string subscriptionId)
        {
            context.UserData.SetValue(ContextConstants.SubscriptionIdKey, subscriptionId);
        }

        public static void Cleanup(this IBotContext context)
        {
            context.UserData.RemoveValue(ContextConstants.SubscriptionIdKey);
            context.UserData.RemoveValue(ContextConstants.AutomationJobsKey);
        }

        public static IList<RunbookJob> GetAutomationJobs(this IBotContext context, string subscriptionId)
        {
            IDictionary<string, IList<RunbookJob>> automationJobsBySubscription;
            IList<RunbookJob> automationJobs;

            if (context.UserData.TryGetValue(ContextConstants.AutomationJobsKey, out automationJobsBySubscription))
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

            if (!context.UserData.TryGetValue(ContextConstants.AutomationJobsKey, out automationJobsBySubscription))
            {
                automationJobsBySubscription = new Dictionary<string, IList<RunbookJob>>();
            }

            automationJobsBySubscription[subscriptionId] = automationJobs;

            context.UserData.SetValue(ContextConstants.AutomationJobsKey, automationJobsBySubscription);
        }

        public static RunbookFormState GetRunbookFormState(this IBotContext context)
        {
            RunbookFormState runbookFormState;

            context.PerUserInConversationData.TryGetValue(ContextConstants.RunbookFormStateKey, out runbookFormState);

            return runbookFormState;
        }

        public static void StoreRunbookFormState(this IBotContext context, RunbookFormState runbookFormState)
        {
            context.PerUserInConversationData.SetValue(ContextConstants.RunbookFormStateKey, runbookFormState);
        }

        public static void CleanupRunbookFormState(this IBotContext context)
        {
            context.PerUserInConversationData.RemoveValue(ContextConstants.RunbookFormStateKey);
        }
    }
}
