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

            if (context.PerUserInConversationData.TryGetValue(ContextConstants.AuthResultKey, out authResult))
            {
                DateTime expires = new DateTime(authResult.ExpiresOnUtcTicks);

                if (DateTime.UtcNow >= expires)
                {
                    try
                    {
                        var result = await AzureActiveDirectoryHelper.GetToken(authResult.UserUniqueId);

                        authResult.AccessToken = result.AccessToken;
                        authResult.ExpiresOnUtcTicks = result.ExpiresOn.UtcTicks;

                        context.StoreAuthResult(authResult);
                    }
                    catch (Exception)
                    {
                        await context.PostAsync("Your credentials expired and could not be renewed automatically!");
                        context.Logout();
                        return null;
                    }
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
            context.PerUserInConversationData.RemoveValue(ContextConstants.SubscriptionIdKey);
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
