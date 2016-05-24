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

                context.Done($"Thanks {user}. You are now logged in.");
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
                context.PerUserInConversationData.SetValue(ContextConstants.PersistedCookieKey, resumptionCookie);

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
