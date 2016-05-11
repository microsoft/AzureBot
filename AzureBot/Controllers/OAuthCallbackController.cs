namespace AzureBot.Controllers
{
    using System;
    using System.Configuration;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Http;
    using Autofac;
    using Helpers;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.Dialogs.Internals;
    using Microsoft.Bot.Connector;
    using Models;

    public class OAuthCallbackController : ApiController
    {
        private static Lazy<string> botId = new Lazy<string>(() => ConfigurationManager.AppSettings["AppId"]);

        [HttpGet]
        [Route("api/OAuthCallback")]
        public async Task<HttpResponseMessage> OAuthCallback([FromUri] string code, [FromUri] string state)
        {
            // Check if the bot is running against emulator
            var connectorType = HttpContext.Current.Request.IsLocal ? ConnectorType.Emulator : ConnectorType.Cloud;

            var resumeInfo = SerializerHelper.DeserializeObject<ResumeState>(state);
            var userId = resumeInfo.UserId;
            var conversationId = resumeInfo.ConversationId;

            // Exchange the Auth code with Access toekn
            var token = await AzureActiveDirectoryHelper.GetTokenByAuthCodeAsync(code);

            // Create the message that is send to conversation to resume the login flow
            var msg = new Message
            {
                Text = $"token:{token.AccessToken}",
                From = new ChannelAccount { Id = userId },
                To = new ChannelAccount { Id = botId.Value },
                ConversationId = conversationId
            };

            // Resume the conversation
            Message reply = await Conversation.ResumeAsync(botId.Value, userId, conversationId, msg, connectorType: connectorType);

            // Remove the pending message because login flow is complete
            IBotData dataBag = new JObjectBotData(reply);
            PendingMessage pending;
            if (dataBag.PerUserInConversationData.TryGetValue("pendingMessage", out pending))
            {
                dataBag.PerUserInConversationData.RemoveValue("pendingMessage");
                var pendingMessage = pending.GetMessage();
                reply.To = pendingMessage.From;
                reply.From = pendingMessage.To;

                // Send the login success asynchronously to user
                var client = Conversation.ResumeContainer.Resolve<IConnectorClient>(TypedParameter.From(connectorType));
                await client.Messages.SendMessageAsync(reply);

                return Request.CreateResponse("You are now logged in! Continue talking to the bot.");
            }
            else
            {
                // Callback is called with no pending message as a result the login flow cannot be resumed.
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, new InvalidOperationException("Cannot resume!"));
            }
        }
    }
}
