namespace AzureBot.Helpers
{
    using System;
    using System.Configuration;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using System.Web;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.IdentityModel.Clients.ActiveDirectory;

    internal static class AzureActiveDirectoryHelper
    {
        private static Lazy<string> activeDirectoryEndpointUrl = new Lazy<string>(() => ConfigurationManager.AppSettings["ActiveDirectory.EndpointUrl"]);
        private static Lazy<string> activeDirectoryTenant = new Lazy<string>(() => ConfigurationManager.AppSettings["ActiveDirectory.Tenant"]);
        private static Lazy<string> activeDirectoryResourceId = new Lazy<string>(() => ConfigurationManager.AppSettings["ActiveDirectory.ResourceId"]);
        private static Lazy<string> redirectUrl = new Lazy<string>(() => ConfigurationManager.AppSettings["ActiveDirectory.RedirectUrl"]);
        private static Lazy<string> clientId = new Lazy<string>(() => ConfigurationManager.AppSettings["ActiveDirectory.ClientId"]);
        private static Lazy<string> clientSecret = new Lazy<string>(() => ConfigurationManager.AppSettings["ActiveDirectory.ClientSecret"]);

        internal static async Task<string> GetAuthUrlAsync(ResumptionCookie resumptionCookie)
        {
            var encodedCookie = UrlToken.Encode(resumptionCookie);

            Uri redirectUri = new Uri(redirectUrl.Value);

            AuthenticationContext context = new AuthenticationContext(activeDirectoryEndpointUrl.Value + "/" + activeDirectoryTenant.Value);

            var uri = await context.GetAuthorizationRequestUrlAsync(
                activeDirectoryResourceId.Value,
                clientId.Value,
                redirectUri, 
                UserIdentifier.AnyUser, 
                "state=" + encodedCookie);

            return uri.ToString();
        }

        internal static async Task<AuthenticationResult> GetTokenByAuthCodeAsync(string authorizationCode, TokenCache tokenCache)
        {
            AuthenticationContext context = new AuthenticationContext(activeDirectoryEndpointUrl.Value + "/" + activeDirectoryTenant.Value, tokenCache);

            Uri redirectUri = new Uri(redirectUrl.Value);

            var result = await context.AcquireTokenByAuthorizationCodeAsync(authorizationCode, redirectUri, new ClientCredential(clientId.Value, clientSecret.Value));

            Trace.TraceInformation("Token Cache Count:" + context.TokenCache.Count);

            return result;
        }

        internal static async Task<AuthenticationResult> GetToken(string userUniqueId, TokenCache tokenCache)
        {
            AuthenticationContext context = new AuthenticationContext(activeDirectoryEndpointUrl.Value + "/" + activeDirectoryTenant.Value, tokenCache);

            return await context.AcquireTokenSilentAsync(activeDirectoryResourceId.Value, new ClientCredential(clientId.Value, clientSecret.Value), new UserIdentifier(userUniqueId, UserIdentifierType.UniqueId));
        }
    }
}
