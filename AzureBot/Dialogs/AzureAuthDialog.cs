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
        private PendingMessage pendingMessage;

        public AzureAuthDialog(Message message)
        {
            this.pendingMessage = new PendingMessage(message);
        }

        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(this.MessageReceivedAsync);
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<Message> argument)
        {
            var result = await AzureActiveDirectoryHelper.GetAuthUrlAsync(this.pendingMessage);

            string loginMessage = string.Format("Welcome to the Azure Bot, your friendly automata to interact with Azure. You are not logged in! In order to start using me, please login using the following url: {0}", result);

            await context.PostAsync(loginMessage);

            context.Wait(this.MessageReceivedAsync);
        }
    }
}
