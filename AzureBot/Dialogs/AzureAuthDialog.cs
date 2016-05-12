namespace AzureBot.Dialogs
{
    using System;
    using System.Threading.Tasks;
    using Helpers;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Connector;
    using Models;

    [Serializable]
    public class AzureAuthDialog : IDialog<string>
    {
        private static readonly string AuthTokenKey = "AuthToken";
        private PendingMessage pendingMessage;

        public AzureAuthDialog(Message msg)
        {
            this.pendingMessage = new PendingMessage(msg);
        }

        public async Task StartAsync(IDialogContext context)
        {
            await this.LogIn(context);
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<Message> argument)
        {
            var msg = await argument;

            if (msg.Text.StartsWith("token:"))
            {
                var token = msg.Text.Remove(0, "token:".Length);
                context.PerUserInConversationData.SetValue(AuthTokenKey, token);
                this.ReturnPendingMessage(context);
            }
            else
            {
                await this.LogIn(context);
            }
        }

        private async Task LogIn(IDialogContext context)
        {
            string token;
            if (!context.PerUserInConversationData.TryGetValue(AuthTokenKey, out token))
            {
                context.PerUserInConversationData.SetValue("pendingMessage", this.pendingMessage);

                var authenticationUrl = await AzureActiveDirectoryHelper.GetAuthUrlAsync(this.pendingMessage);

                await context.PostAsync($"You must be authenticated in Azure to access your subscription. Please, use the following url to log into your Azure account: {authenticationUrl}");

                context.Wait(this.MessageReceivedAsync);
            }
            else
            {
                this.ReturnPendingMessage(context);
            }
        }

        private void ReturnPendingMessage(IDialogContext context)
        {
            context.Done(this.pendingMessage.Text);
        }
    }
}
