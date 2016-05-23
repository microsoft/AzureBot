namespace AzureBot
{
    using System;
    using System.Threading.Tasks;
    using Helpers;
    using Microsoft.Bot.Builder.Dialogs;
    using Models;

    public static class ContextExtensions
    {
        public static async Task<string> GetAccessToken(this IBotContext context)
        {
            AuthResult authResult;

            if (context.PerUserInConversationData.TryGetValue<AuthResult>(ContextConstants.AuthResultKey, out authResult))
            {
                DateTime expires = new DateTime(authResult.ExpiresOnUtcTicks);

                if (DateTime.UtcNow >= expires)
                {
                    var result = await AzureActiveDirectoryHelper.GetToken(authResult.UserDisplayableId);

                    authResult.AccessToken = result.AccessToken;
                    authResult.ExpiresOnUtcTicks = result.ExpiresOn.UtcTicks;

                    context.StoreAuthResult(authResult);
                }

                return authResult.AccessToken;
            }

            return null;
        }

        public static void StoreAuthResult(this IBotContext context, AuthResult authResult)
        {
            context.PerUserInConversationData.SetValue(ContextConstants.AuthResultKey, authResult);
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
