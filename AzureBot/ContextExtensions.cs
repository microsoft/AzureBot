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
                    var result = await AzureActiveDirectoryHelper.GetToken(authResult.UserUniqueId);

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

        public static void Logout(this IBotContext context)
        {
            context.PerUserInConversationData.RemoveValue(ContextConstants.AuthResultKey);
        }

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
