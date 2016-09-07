namespace AzureBot.Dialogs
{
    using AuthBot;
    using AuthBot.Dialogs;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.Dialogs.Internals;
    using Microsoft.Bot.Builder.FormFlow;
    using Microsoft.Bot.Builder.Luis;
    using Microsoft.Bot.Builder.Luis.Models;
    using Microsoft.Bot.Connector;
    using System;
    using System.Configuration;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    [LuisModel("d2129bee-5d15-4c78-be3b-2005e3c08cd4", "0e64d2ae951547f692182b4ae74262cb")]
    [Serializable]
    public class RootDialog : LuisDialog<string>
    {
        private static Lazy<string> resourceId = new Lazy<string>(() => ConfigurationManager.AppSettings["ActiveDirectory.ResourceId"]);
        private bool serviceUrlSet = false;
        private string userToBot;
        [LuisIntent("")]
        [LuisIntent("None")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            string message = $"Sorry, I did not understand '{result.Query}'. Type 'help' if you need assistance.";

            await context.PostAsync(message);

            context.Wait(MessageReceived);
        }

        [LuisIntent("Help")]
        public async Task Help(IDialogContext context, LuisResult result)
        {
            string message = "";
            var accessToken = await context.GetAccessToken(resourceId.Value);
            if (string.IsNullOrEmpty(accessToken))
            {
                message += $"Hello!\n\n";
            }
            message += "I can help you: \n";
            message += $"* List, Switch and Select an Azure subscription\n";
            message += $"* List, Start, Shutdown (power off your VM, still incurring compute charges), and Stop (deallocates your VM, no charges) your virtual machines\n";
            message += $"* List your automation accounts and your runbooks\n";
            message += $"* Start a runbook, get the description of a runbook, get the status and output of automation jobs\n";
            message += $"* Login and Logout of an Azure Subscription\n\n";

            if (string.IsNullOrEmpty(accessToken))
            {
                message += $"By using me, you agree to the Microsoft Privacy Statement and Microsoft Services Agreement on http://aka.ms/AzureBot \n\n";
                message += $"Please type **login** to interact with me for the first time.";
            }
            

            await context.PostAsync(message);

            context.Wait(this.MessageReceived);
        }

        protected override async Task MessageReceived(IDialogContext context, IAwaitable<IMessageActivity> item)
        {
            var message = await item;

            //No way to get the message in the LuisIntent methods so saving it here
            userToBot = message.Text.ToLowerInvariant();

            if (!serviceUrlSet)
            {
                context.PrivateConversationData.SetValue("ServiceUrl", message.ServiceUrl);
                serviceUrlSet = true;
            }
            if (userToBot.Contains("help"))
            {
                await base.MessageReceived(context, item);

                return;
            }

            var accessToken = await context.GetAccessToken(resourceId.Value);

            if (string.IsNullOrEmpty(accessToken))
            {
                if (userToBot.Contains("login"))
                {
                    await context.Forward(new AzureAuthDialog(resourceId.Value), this.ResumeAfterAuth, message, CancellationToken.None);
                }
                else
                {
                    await this.Help(context, new LuisResult());
                }
            }
            else
            {
                if (string.IsNullOrEmpty(context.GetSubscriptionId()))
                {
                    await this.UseSubscriptionAsync(context, new LuisResult());
                }
                else
                {
                    await base.MessageReceived(context, item);
                }
            }
        }

        [LuisIntent("ListSubscriptions")]
        public async Task ListSubscriptionsAsync(IDialogContext context, LuisResult result)
        {
            var accessToken = await context.GetAccessToken(resourceId.Value);
            if (string.IsNullOrEmpty(accessToken))
            {
                return;
            }

            var subscriptions = await Domain.Subscription.ListSubscriptionsAsync(accessToken);
            
            int index = 0;
            var subscriptionsText = subscriptions.Aggregate(
                string.Empty,
                (current, next) =>
                    {
                        index++;
                        return current += $"\r\n{index}. {next.DisplayName}";
                    });

            await context.PostAsync($"Your subscriptions are: {subscriptionsText}");

            context.Wait(MessageReceived);
        }

        [LuisIntent("CurrentSubscription")]
        public async Task GetCurrentSubscriptionAsync(IDialogContext context, LuisResult result)
        {
            var accessToken = await context.GetAccessToken(resourceId.Value);
            if (string.IsNullOrEmpty(accessToken))
            {
                return;
            }

            var subscriptionId = context.GetSubscriptionId();

            var currentSubscription = await Domain.Subscription.GetSubscription(accessToken, subscriptionId);

            await context.PostAsync($"Your current subscription is '{currentSubscription.DisplayName}'.");

            context.Wait(MessageReceived);
        }

        [LuisIntent("UseSubscription")]
        public async Task UseSubscriptionAsync(IDialogContext context, LuisResult result)
        {
            EntityRecommendation subscriptionEntity;

            var accessToken = await context.GetAccessToken(resourceId.Value);
            
            if (string.IsNullOrEmpty(accessToken))
            {
                context.Wait(MessageReceived);
                return;
            }

            var availableSubscriptions = await Domain.Subscription.ListSubscriptionsAsync(accessToken);

            // check if the user specified a subscription name in the command
            if (result.TryFindEntity("Subscription", out subscriptionEntity))
            {
                // obtain the name specified by the user - text in LUIS result is different
                var subscriptionName = subscriptionEntity.GetEntityOriginalText(result.Query);

                // ensure that the subscription exists
                var selectedSubscription = availableSubscriptions.FirstOrDefault(p => p.DisplayName.Equals(subscriptionName, StringComparison.InvariantCultureIgnoreCase));
                if (selectedSubscription == null)
                {
                    await context.PostAsync($"The '{subscriptionName}' subscription was not found.");
                    context.Wait(this.MessageReceived);
                    return;
                }

                subscriptionEntity.Entity = selectedSubscription.DisplayName;
                subscriptionEntity.Type = "SubscriptionId";
            }

            var formState = new Forms.SubscriptionFormState(availableSubscriptions);

            if (availableSubscriptions.Count() == 1)
            {
                formState.SubscriptionId = availableSubscriptions.Single().SubscriptionId;
                formState.DisplayName = availableSubscriptions.Single().DisplayName;
            }

            var form = new FormDialog<Forms.SubscriptionFormState>(
                formState,
                Forms.EntityForms.BuildSubscriptionForm,
                FormOptions.PromptInStart,
                result.Entities);

            context.Call(form, this.UseSubscriptionFormComplete);
        }


        [LuisIntent("Logout")]
        public async Task Logout(IDialogContext context, LuisResult result)
        {
            context.Cleanup();
            await context.Logout();

            context.Wait(this.MessageReceived);
        }

        [LuisIntent("DetermineResource")]
        public async Task DetermineResourceAsync(IDialogContext context, LuisResult result)
        {
            var accessToken = await context.GetAccessToken(resourceId.Value);
            if (string.IsNullOrEmpty(accessToken))
            {
                return;
            }

            //var entity = result.Entities.First();
            //if (string.IsNullOrEmpty(entity.Entity))
            //{
            //    await None(context, result);
            //}
            //else
            //{
            var message = context.MakeMessage();
            message.Text = userToBot;

            if (result.Query.ToLowerInvariant().Contains("vm"))
                await context.Forward(new VMDialog(), this.ResumeAfterForward, message, CancellationToken.None);
            else
            if (result.Query.ToLowerInvariant().Contains("job") | result.Query.ToLowerInvariant().Contains("runbook"))
                await context.Forward(new AutomationDialog(), this.ResumeAfterForward, message, CancellationToken.None);
            //}
        }

        private async Task ResumeAfterForward(IDialogContext context, IAwaitable<string> result)
        {
            var message = await result;
            if (string.IsNullOrEmpty(message))
                context.Wait(this.MessageReceived); 
            else
                await None(context, new LuisResult()); //the second dialog didn't understand the command
        }

        private async Task ResumeAfterAuth(IDialogContext context, IAwaitable<string> result)
        {
            var message = await result;

            await context.PostAsync(message);

            await this.UseSubscriptionAsync(context, new LuisResult());
        }

        private async Task UseSubscriptionFormComplete(IDialogContext context, IAwaitable<Forms.SubscriptionFormState> result)
        {
            try
            {
                var subscriptionFormState = await result;
                if (!string.IsNullOrEmpty(subscriptionFormState.SubscriptionId))
                {
                    var selectedSubscription = subscriptionFormState.AvailableSubscriptions.Single(sub => sub.SubscriptionId == subscriptionFormState.SubscriptionId);
                    context.StoreSubscriptionId(subscriptionFormState.SubscriptionId);
                    await context.PostAsync($"Setting {selectedSubscription.DisplayName} as the current subscription. What would you like to do next?");
                    context.Wait(this.MessageReceived);
                }
                else
                {
                    PromptDialog.Confirm(
                        context,
                        this.OnLogoutRequested,
                        "Oops! You don't have any Azure subscriptions under the account you used to log in. To continue using the bot, log in with a different account. Do you want to log out and start over?",
                        "Didn't get that!",
                        promptStyle: PromptStyle.None);
                }
            }
            catch (FormCanceledException<Forms.SubscriptionFormState> e)
            {
                string reply;

                if (e.InnerException == null)
                {
                    reply = "You have canceled the operation. What would you like to do next?";
                }
                else
                {
                    reply = $"Oops! Something went wrong :(. Technical Details: {e.InnerException.Message}";
                }

                await context.PostAsync(reply);
                context.Wait(this.MessageReceived);
            }
        }

        private async Task OnLogoutRequested(IDialogContext context, IAwaitable<bool> confirmation)
        {
            var result = await confirmation;

            if (result)
            {
                context.Cleanup();
                await context.Logout();
            }

            context.Wait(this.MessageReceived);
        }
    }
}
