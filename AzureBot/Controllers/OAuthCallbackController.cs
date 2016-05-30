namespace AzureBot.Controllers
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Web.Http;
    using Autofac;
    using Helpers;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.Dialogs.Internals;
    using Microsoft.Bot.Connector;
    using Microsoft.IdentityModel.Clients.ActiveDirectory;
    using Models;

    public class OAuthCallbackController : ApiController
    {
        [HttpGet]
        [Route("api/OAuthCallback")]
        public async Task<HttpResponseMessage> OAuthCallback([FromUri] string code, [FromUri] string state)
        {
            try
            {
                // Get the resumption cookie
                var resumptionCookie = UrlToken.Decode<ResumptionCookie>(state);

                var tokenCache = new TokenCache();

                // Exchange the Auth code with Access token
                var token = await AzureActiveDirectoryHelper.GetTokenByAuthCodeAsync(code, tokenCache);

                // Create the message that is send to conversation to resume the login flow
                var message = resumptionCookie.GetMessage();
               
                using (var scope = DialogModule.BeginLifetimeScope(Conversation.Container, message))
                {
                    var client = scope.Resolve<IConnectorClient>();

                    var tokenCacheBlob = tokenCache.Serialize();

                    AuthResult authResult = new AuthResult
                    {
                        AccessToken = token.AccessToken,
                        UserName = $"{token.UserInfo.GivenName} {token.UserInfo.FamilyName}", 
                        UserUniqueId = token.UserInfo.UniqueId,
                        ExpiresOnUtcTicks = token.ExpiresOn.UtcTicks,
                        TokenCache = tokenCacheBlob
                    };

                    var data = await client.Bots.GetPerUserConversationDataAsync(resumptionCookie.BotId, resumptionCookie.ConversationId, resumptionCookie.UserId);

                    data.SetProperty(ContextConstants.AuthResultKey, authResult);

                    await client.Bots.SetPerUserInConversationDataAsync(resumptionCookie.BotId, resumptionCookie.ConversationId, resumptionCookie.UserId, data);

                    var reply = await Conversation.ResumeAsync(resumptionCookie, message);

                    reply.To = message.From;
                    reply.From = message.To;

                    await client.Messages.SendMessageAsync(reply);
                }

                return Request.CreateResponse("You are now logged in! Continue talking to the bot.");
            }
            catch
            {
                // Callback is called with no pending message as a result the login flow cannot be resumed.
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, new InvalidOperationException("Cannot resume!"));
            }
        }
    }
}
