namespace AzureBot.Dialogs
{
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Connector;

    public class AuthDialog : IDialog<Message>
    {
        public const string AuthTokenKey = "AuthToken";

        public async Task StartAsync(IDialogContext context)
        {
            await LogIn(context);
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<Message> argument)
        {
            var authToken = await (argument);
            context.PerUserInConversationData.SetValue(AuthTokenKey, authToken);
            context.Done(authToken);
        }

        private async Task LogIn(IDialogContext context)
        {
            string token;
            if (!context.PerUserInConversationData.TryGetValue(AuthTokenKey, out token))
            {
                await context.PostAsync("Waiting for authentication...");
                context.Wait(MessageReceivedAsync);
            }
            else
            {
                context.Done(token);
            }
        }
    }
}