namespace AzureBot
{
    using Dialogs;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.Luis;
    using Microsoft.Bot.Connector;
    using System.Configuration;
    using System.Diagnostics;
    using System.Linq;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web.Http;

    [BotAuthentication]
    public class MessagesController : ApiController
    {
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public virtual async Task<HttpResponseMessage> Post([FromBody] Activity activity)
        {
            if (activity != null)
            {
                StateClient stateClient = activity.GetStateClient();

                switch (activity.GetActivityType())
                {
                    case ActivityTypes.Event:
                        var eventToken = activity.Value.ToString();

                        AuthBot.Models.AuthResult authResult = new AuthBot.Models.AuthResult();
                        object tokenCache = new Microsoft.IdentityModel.Clients.ActiveDirectory.TokenCache();
                        var token = await AuthBot.Helpers.AzureActiveDirectoryHelper.GetTokenByAuthCodeAsync(eventToken,
                            (Microsoft.IdentityModel.Clients.ActiveDirectory.TokenCache)tokenCache);

                        BotData userData = await stateClient.BotState.GetUserDataAsync(activity.ChannelId, activity.From.Id);
                        userData.SetProperty(ContextConstants.AuthResultKey, token);
                        await stateClient.BotState.SetUserDataAsync(activity.ChannelId, activity.From.Id, userData);
                        break;
                    case ActivityTypes.Message:
                    case ActivityTypes.ConversationUpdate:

                        if (!string.IsNullOrEmpty(activity.Text) && 
                            new[] { "cancel", "reset", "start over", "/deleteprofile" }.Any(c => activity.Text.Contains(c)))
                        {
                            await stateClient.BotState.DeleteStateForUserAsync(activity.ChannelId,
                                                                        activity.From.Id,
                                                                        CancellationToken.None);
                        }
                        await Conversation.SendAsync(activity, () => new RootDialog(new LuisService(new LuisModelAttribute(ConfigurationManager.AppSettings["RootDialog.AppId"],
                                                                                                   ConfigurationManager.AppSettings["LuisAPIKey"]))));
                        break;
                    default:
                        Trace.TraceError($"Azure Bot ignored an activity. Activity type received: {activity.GetActivityType()}");
                        break;
                }
            }
            return new HttpResponseMessage(System.Net.HttpStatusCode.Accepted);
        }

    }
}