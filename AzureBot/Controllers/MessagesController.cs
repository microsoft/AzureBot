namespace AzureBot
{
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using System.Web.Http;
    using Azure.Management.ResourceManagement;
    using Dialogs;
    using Forms;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.FormFlow;
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
                .Switch<Message, IDialog<string>>(
                    new Case<Message, IDialog<string>>(
                        (message) =>
                            {
                                var regex = new Regex("^help", RegexOptions.IgnoreCase);
                                return regex.IsMatch(message.Text);
                            }, 
                        (ctx, message) => Chain.ContinueWith(new ActionDialog(message.Text, true), AzureActionsDialogCallback)),
                    new DefaultCase<Message, IDialog<string>>((context, message) =>
                    {
                        return Chain.ContinueWith<string, string>(new AzureAuthDialog(message), AzureAuthDialogContinuation)
                        .ContinueWith<string, string>(AzureSubscriptionDialogCallback)
                        .ContinueWith<string, string>(AzureActionsDialogCallback)
                        .Loop();
                    })).Unwrap();
        }

        private static async Task<IDialog<string>> AzureAuthDialogContinuation(IBotContext context, IAwaitable<string> item)
        {
            var msg = await item;
            if (!string.IsNullOrEmpty(msg))
            {
                await context.PostAsync(msg);
            }

            return Chain.Return(msg);
        }

        private static async Task<IDialog<string>> AzureSubscriptionDialogCallback(IBotContext context, IAwaitable<string> message)
        {
            var msg = await message;

            var accessToken = await context.GetAccessToken();

            var availableSubscriptions = await new AzureRepository().ListSubscriptionsAsync(accessToken);

            var form = new FormDialog<SubscriptionFormState>(
                new SubscriptionFormState(availableSubscriptions),
                EntityForms.BuildSubscriptionForm,
                FormOptions.PromptInStart);

            return Chain.ContinueWith(form, AzureSubscriptionDialogContinuation);
        }

        private static async Task<IDialog<string>> AzureSubscriptionDialogContinuation(IBotContext context, IAwaitable<SubscriptionFormState> result)
        {
            var subscriptionFormState = await result;

            if (string.IsNullOrEmpty(subscriptionFormState.SubscriptionId))
            {
                string prompt = "Oops! You don't have any Azure subscriptions under the account you used to log in. To continue using the bot, log in with a different account. Do you want to log out and start over?";
                return Chain.ContinueWith<bool, string>(new PromptDialog.PromptConfirm(prompt, prompt, 3), OnLogoutRequested);
            }

            context.StoreSubscriptionId(subscriptionFormState.SubscriptionId);

            return Chain.Return(subscriptionFormState.DisplayName);
        }

        private static async Task<IDialog<string>> OnLogoutRequested(IBotContext context, IAwaitable<bool> confirmation)
        {
            var result = await confirmation;

            if (result)
            {
                context.Logout();
            }

            return Chain.Return(string.Empty);
        }

        private static async Task<IDialog<string>> AzureActionsDialogCallback(IBotContext context, IAwaitable<string> message)
        {
            string msg;

            if (string.IsNullOrEmpty(context.GetSubscriptionId()))
            {
                return Chain.Return(string.Empty);
            }

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