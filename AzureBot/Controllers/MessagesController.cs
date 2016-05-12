namespace AzureBot
{
    using System.Threading.Tasks;
    using System.Web.Http;
    using Dialogs;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Connector;

    [BotAuthentication]
    public class MessagesController : ApiController
    {
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<Message> Post([FromBody]Message message)
        {
            if (message.Type == "Message")
            {
                return await Conversation.SendAsync(message, MakeRoot);
            }
            else
            {
                return this.HandleSystemMessage(message);
            }
        }

        private static IDialog<string> MakeRoot()
        {
            return Chain.PostToChain()
                .ContinueWith<Message, string>(AzureAuthDialogCallback)
                .PostToUser()
                //// .ContinueWith<string, string>(AzureSubscriptionDialogCallback)
                .ContinueWith<string, string>(AzureActionsDialogCallback);
        }

        private static async Task<IDialog<string>> AzureAuthDialogCallback(IBotContext context, IAwaitable<Message> message)
        {
            var msg = await message;

            return Chain.ContinueWith<string, string>(new AzureAuthDialog(msg), AzureAuthDialogContinuation);
        }

        private static async Task<IDialog<string>> AzureAuthDialogContinuation(IBotContext context, IAwaitable<string> item)
        {
            var msg = await item;

            return Chain.Return(msg);
        }

        private static async Task<IDialog<string>> AzureActionsDialogCallback(IBotContext context, IAwaitable<string> message)
        {
            var msg = await message;

            return Chain.ContinueWith<string, string>(new ActionDialog(msg), AzureActionsDialogContinuation);
        }

        private static async Task<IDialog<string>> AzureActionsDialogContinuation(IBotContext context, IAwaitable<string> item)
        {
            var msg = await item;

            return Chain.Return(msg);
        }

        private Message HandleSystemMessage(Message message)
        {
            if (message.Type == "Ping")
            {
                Message reply = message.CreateReplyMessage();
                reply.Type = "Ping";
                return reply;
            }
            else if (message.Type == "DeleteUserData")
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == "BotAddedToConversation")
            {
            }
            else if (message.Type == "BotRemovedFromConversation")
            {
            }
            else if (message.Type == "UserAddedToConversation")
            {
            }
            else if (message.Type == "UserRemovedFromConversation")
            {
            }
            else if (message.Type == "EndOfConversation")
            {
            }

            return null;
        }
    }
}