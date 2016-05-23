namespace AzureBot.Helpers
{
    using System;
    using System.Configuration;
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
            var serializedCookie = resumptionCookie.GZipSerialize();

            Uri redirectUri = new Uri(redirectUrl.Value);

            AuthenticationContext context = new AuthenticationContext(activeDirectoryEndpointUrl.Value + "/" + activeDirectoryTenant.Value);

            var uri = await context.GetAuthorizationRequestUrlAsync(
                activeDirectoryResourceId.Value,
                clientId.Value,
                redirectUri, 
                UserIdentifier.AnyUser, 
                "state=" + HttpUtility.UrlEncode(serializedCookie));

            return uri.ToString();
        }

        internal static async Task<AuthenticationResult> GetTokenByAuthCodeAsync(string authorizationCode)
        {
            AuthenticationContext context = new AuthenticationContext(activeDirectoryEndpointUrl.Value + "/" + activeDirectoryTenant.Value);

            Uri redirectUri = new Uri(redirectUrl.Value);

            return await context.AcquireTokenByAuthorizationCodeAsync(authorizationCode, redirectUri, new ClientCredential(clientId.Value, clientSecret.Value));
        }

        internal static async Task<AuthenticationResult> GetToken(string userUniqueId)
        {
            AuthenticationContext context = new AuthenticationContext(activeDirectoryEndpointUrl.Value + "/" + activeDirectoryTenant.Value);

            Uri redirectUri = new Uri(redirectUrl.Value);

            return await context.AcquireTokenSilentAsync(activeDirectoryResourceId.Value, new ClientCredential(clientId.Value, clientSecret.Value), new UserIdentifier(userUniqueId, UserIdentifierType.UniqueId));
        }
    }
}
