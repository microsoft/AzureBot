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
        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(this.MessageReceivedAsync);
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<Message> argument)
        {
            var msg = await argument;

            AuthResult authResult;
            if (context.PerUserInConversationData.TryGetValue(ContextConstants.AuthResultKey, out authResult))
            {
                context.Done($"Thanks {authResult.UserName}. You are now logged in.");
            }
            else
            {
                await this.LogIn(context, msg);
            }
        }

        private async Task LogIn(IDialogContext context, Message msg)
        {
            string token = await context.GetAccessToken();

            if (string.IsNullOrEmpty(token))
            {
                var resumptionCookie = new ResumptionCookie(msg);

                var authenticationUrl = await AzureActiveDirectoryHelper.GetAuthUrlAsync(resumptionCookie);

                await context.PostAsync($"You must be authenticated in Azure to access your subscription. Please, click [here]({authenticationUrl}) to log into your Azure account.");

                context.Wait(this.MessageReceivedAsync);
            }
            else
            {
                context.Done(string.Empty);
            }
        }
    }
}
