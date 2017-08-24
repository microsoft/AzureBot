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
                switch (activity.GetActivityType())
                {
                    case ActivityTypes.Message:
                    case ActivityTypes.ConversationUpdate:

                        if (!string.IsNullOrEmpty(activity.Text) && 
                            new[] { "cancel", "reset", "start over", "/deleteprofile" }.Any(c => activity.Text.Contains(c)))
                        {
                            StateClient stateClient = activity.GetStateClient();
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