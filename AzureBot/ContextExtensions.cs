namespace AzureBot
{
    using Microsoft.Bot.Builder.Dialogs;

    public static class ContextExtensions
    {
        public static string GetAccessToken(this IBotContext context)
        {
            return context.PerUserInConversationData.Get<string>(ContextConstants.AuthTokenKey);
        }

        public static string GetSubscriptionId(this IBotContext context)
        {
            return context.PerUserInConversationData.Get<string>(ContextConstants.SubscriptionIdKey);
        }

        public static void StoreSubscriptionId(this IBotContext context, string subscriptionId)
        {
            context.PerUserInConversationData.SetValue(ContextConstants.SubscriptionIdKey, subscriptionId);
        }
    }
}
