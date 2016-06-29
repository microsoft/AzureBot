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

        public static IList<RunbookJob> GetAutomationJobs(this IBotContext context)
        {
            List<RunbookJob> automationJobs;

            context.UserData.TryGetValue(ContextConstants.AutomationJobsKey, out automationJobs);

            return automationJobs;
        }

        public static void StoreAutomationJobs(this IBotContext context, IList<RunbookJob> automationJobs)
        {
            context.UserData.SetValue(ContextConstants.AutomationJobsKey, automationJobs);
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
