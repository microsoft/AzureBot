namespace AzureBot.Helpers
{
    using System;
    using System.Configuration;
    using System.Threading.Tasks;
    using System.Web;
    using Microsoft.IdentityModel.Clients.ActiveDirectory;
    using Models;

    internal static class AzureActiveDirectoryHelper
    {
        private static Lazy<string> activeDirectoryEndpointUrl = new Lazy<string>(() => ConfigurationManager.AppSettings["ActiveDirectoryEndpointUrl"]);
        private static Lazy<string> activeDirectoryTenant = new Lazy<string>(() => ConfigurationManager.AppSettings["ActiveDirectoryTenant"]);
        private static Lazy<string> activeDirectoryResourceId = new Lazy<string>(() => ConfigurationManager.AppSettings["ActiveDirectoryResourceId"]);
        private static Lazy<string> redirectUrl = new Lazy<string>(() => ConfigurationManager.AppSettings["RedirectUrl"]);
        private static Lazy<string> clientId = new Lazy<string>(() => ConfigurationManager.AppSettings["ClientId"]);
        private static Lazy<string> clientSecret = new Lazy<string>(() => ConfigurationManager.AppSettings["ClientSecret"]);

        internal static async Task<string> GetAuthUrlAsync(PendingMessage pendingMessage)
        {
            var state = new ResumeState { UserId = pendingMessage.userId, ConversationId = pendingMessage.conversationId };

            var serializedState = SerializerHelper.SerializeObject(state);

            Uri redirectUri = new Uri(redirectUrl.Value);

            AuthenticationContext context = new AuthenticationContext(activeDirectoryEndpointUrl.Value + "/" + activeDirectoryTenant.Value);

            var uri = await context.GetAuthorizationRequestUrlAsync(
                activeDirectoryResourceId.Value,
                clientId.Value,
                redirectUri, 
                UserIdentifier.AnyUser, 
                "state=" + HttpUtility.UrlEncode(serializedState));

            return uri.ToString();
        }

        internal static async Task<AuthenticationResult> GetTokenByAuthCodeAsync(string authorizationCode)
        {
            AuthenticationContext context = new AuthenticationContext(activeDirectoryEndpointUrl.Value + "/" + activeDirectoryTenant.Value);

            Uri redirectUri = new Uri(redirectUrl.Value);

            return await context.AcquireTokenByAuthorizationCodeAsync(authorizationCode, redirectUri, new ClientCredential(clientId.Value, clientSecret.Value));
        }
    }
}
