namespace AzureBot
{
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using Helpers;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.IdentityModel.Clients.ActiveDirectory;
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
                    Trace.TraceInformation("Token Expired");

                    try
                    {
                        TokenCache tokenCache = new TokenCache(authResult.TokenCache);

                        Trace.TraceInformation("Trying to renew token...");
                        var result = await AzureActiveDirectoryHelper.GetToken(authResult.UserUniqueId, tokenCache);

                        authResult.AccessToken = result.AccessToken;
                        authResult.ExpiresOnUtcTicks = result.ExpiresOn.UtcTicks;
                        authResult.TokenCache = tokenCache.Serialize();

                        context.StoreAuthResult(authResult);

                        Trace.TraceInformation("Token renewed!");
                    }
                    catch (Exception ex)
                    {
                        Trace.TraceError("Failed to renew token: " + ex.Message);

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
