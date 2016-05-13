namespace AzureBot
{
    using System.Threading.Tasks;
    using System.Web.Http;
    using Dialogs;
    using FormTemplates;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.FormFlow;
    using Microsoft.Bot.Connector;

    [BotAuthenticationFromSetting("BotFramework.AppId", "BotFramework.AppSecret")]
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
                .ContinueWith<Message, string>(MessageDialogCallback)
                .ContinueWith<string, string>(AzureSubscriptionDialogCallback)
                .ContinueWith<string, string>(AzureActionsDialogCallback);
        }

        private static async Task<IDialog<string>> MessageDialogCallback(IBotContext context, IAwaitable<Message> message)
        {
            var msg = await message;

            return Chain.ContinueWith<string, string>(new AzureAuthDialog(msg), AzureAuthDialogContinuation);
        }

        private static async Task<IDialog<string>> AzureAuthDialogContinuation(IBotContext context, IAwaitable<string> item)
        {
            var msg = await item;
            if (msg.Contains("Thanks"))
            {
                await context.PostAsync(msg);
            }
            
            return Chain.Return(msg);
        }

        private static async Task<IDialog<string>> AzureSubscriptionDialogCallback(IBotContext context, IAwaitable<string> message)
        {
            var msg = await message;

            return Chain.ContinueWith<SubscriptionFormState, string>(FormDialog.FromForm(EntityForms.BuildSubscriptionForm, FormOptions.PromptInStart), AzureSubscriptionDialogContinuation);
        }

        private static async Task<IDialog<string>> AzureSubscriptionDialogContinuation(IBotContext context, IAwaitable<SubscriptionFormState> item)
        {
            var msg = await item;
            context.PerUserInConversationData.SetValue(ContextConstants.SubscriptionIdKey, msg.SubscriptionId);

            return Chain.Return(msg.DisplayName);
        }

        private static async Task<IDialog<string>> AzureActionsDialogCallback(IBotContext context, IAwaitable<string> message)
        {
            string msg;

            if (context.PerUserInConversationData.TryGetValue(ContextConstants.OriginalMessageKey, out msg))
            {
                context.PerUserInConversationData.RemoveValue(ContextConstants.OriginalMessageKey);
            }
            else
            {
                msg = await message;
            }


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