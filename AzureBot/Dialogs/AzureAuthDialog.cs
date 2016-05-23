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
        private readonly ResumptionCookie resumptionCookie;

        public AzureAuthDialog(Message msg)
        {
            this.resumptionCookie = new ResumptionCookie(msg);
        }

        public async Task StartAsync(IDialogContext context)
        {
            await this.LogIn(context);
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<Message> argument)
        {
            var msg = await argument;

            if (msg.Text.StartsWith("token&"))
            {
                string[] messageParts = msg.Text.Split('&');
                var token = messageParts[1];
                var user = messageParts[2];
                var userUniqueId = messageParts[3];
                var expiresOn = messageParts[4];

                AuthResult authResult = new AuthResult
                {
                    AccessToken = token,
                    UserUniqueId = userUniqueId,
                    ExpiresOnUtcTicks = long.Parse(expiresOn)
                };

                context.StoreAuthResult(authResult);
                
                context.Done($"Thanks {user}. You are now logged in. What do you want to do next?");
            }
            else
            {
                context.PerUserInConversationData.SetValue(ContextConstants.OriginalMessageKey, msg.Text);
                await this.LogIn(context);
            }
        }

        private async Task LogIn(IDialogContext context)
        {
            string token = await context.GetAccessToken();

            if (string.IsNullOrEmpty(token))
            {
                context.PerUserInConversationData.SetValue(ContextConstants.PersistedCookieKey, this.resumptionCookie);

                var authenticationUrl = await AzureActiveDirectoryHelper.GetAuthUrlAsync(this.resumptionCookie);

                await context.PostAsync($"You must be authenticated in Azure to access your subscription. Please, click [here]({authenticationUrl}) to log into your Azure account.");

                context.Wait(this.MessageReceivedAsync);
            }
            else
            {
                this.ReturnPendingMessage(context);
            }
        }

        private void ReturnPendingMessage(IDialogContext context)
        {
            context.Done(string.Empty);
        }
    }
}
