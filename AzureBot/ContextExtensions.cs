namespace AzureBot
{
    using Microsoft.Bot.Builder.Dialogs;

    public static class ContextExtensions
    {
        public static string GetSubscriptionId(this IBotContext context)
        {
            string subscriptionId;

            context.PerUserInConversationData.TryGetValue<string>(ContextConstants.SubscriptionIdKey, out subscriptionId);

            return subscriptionId;
        }

        public static void StoreSubscriptionId(this IBotContext context, string subscriptionId)
        {
            context.PerUserInConversationData.SetValue(ContextConstants.SubscriptionIdKey, subscriptionId);
        }
    }
}
