namespace AzureBot.Helpers
{
    using System;
    using System.Configuration;
    using System.IO;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Threading.Tasks;
    using Microsoft.IdentityModel.Clients.ActiveDirectory;
    using Models;
    using System.Web;
    public static class AzureActiveDirectoryHelper
    {
        private static Lazy<string> ActiveDirectoryEndpointUrl = new Lazy<string>(() => ConfigurationManager.AppSettings["ActiveDirectoryEndpointUrl"]);
        private static Lazy<string> ActiveDirectoryTenant = new Lazy<string>(() => ConfigurationManager.AppSettings["ActiveDirectoryTenant"]);
        private static Lazy<string> ActiveDirectoryResourceId = new Lazy<string>(() => ConfigurationManager.AppSettings["ActiveDirectoryResourceId"]);
        private static Lazy<string> RedirectUrl = new Lazy<string>(() => ConfigurationManager.AppSettings["RedirectUrl"]);
        private static Lazy<string> ClientId = new Lazy<string>(() => ConfigurationManager.AppSettings["ClientId"]);

        internal static async Task<string> GetAuthUrlAsync(PendingMessage pendingMessage)
        {
            var state = new ResumeState { UserId = pendingMessage.userId, ConversationId = pendingMessage.conversationId };

            var serializedState = SerializerHelper.SerializeObject(state);

            Uri redirectUri = new Uri(RedirectUrl.Value);

            AuthenticationContext context = new AuthenticationContext(ActiveDirectoryEndpointUrl.Value + "/" + ActiveDirectoryTenant.Value);

            var uri = await context.GetAuthorizationRequestUrlAsync(ActiveDirectoryResourceId.Value,
                ClientId.Value,
                redirectUri, UserIdentifier.AnyUser, "state=" + HttpUtility.UrlEncode(serializedState));

            return uri.ToString();
        }
    }
}
