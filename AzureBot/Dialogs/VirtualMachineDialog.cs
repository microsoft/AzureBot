namespace AzureBot.Dialogs
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Connector;

    [Serializable]
    public class VirtualMachineDialog : IDialog<string>
    {
        private readonly string originalMessage;

        public VirtualMachineDialog(string originalMessage)
        {
            this.originalMessage = originalMessage;
        }

        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(this.MessageReceivedAsync);
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<Message> argument)
        {
            var message = await argument;
            await context.PostAsync($"Original message: {this.originalMessage} - Received message: {message.Text}");
        }
    }
}