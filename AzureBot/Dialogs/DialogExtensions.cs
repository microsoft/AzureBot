namespace AzureBot.Dialogs
{
    using System;
    using System.Threading.Tasks;
    using Autofac;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.Dialogs.Internals;
    using Microsoft.Bot.Connector;

    public static class DialogExtensions
    {
        public static void NotifyLongRunningOperation<T>(this Task<T> operation, IDialogContext context, Func<T, IDialogContext, string> handler)
        {
            ResumptionCookie resumptionCookie;

            ////context.PostAsync("This operation may take some time. You may continue with other tasks. You'll be notified when the operation is complete.");
            if (context.PerUserInConversationData.TryGetValue(ContextConstants.PersistedCookieKey, out resumptionCookie))
            {
                operation.ContinueWith(
                    async (t, ctx) =>
                    {
                        var reply = resumptionCookie.GetMessage();
                        var to = reply.To;
                        reply.To = reply.From;
                        reply.From = to;
                        reply.Text = handler(t.Result, (IDialogContext)ctx);

                        using (var scope = DialogModule.BeginLifetimeScope(Conversation.Container, reply))
                        {
                            var client = scope.Resolve<IConnectorClient>();
                            await client.Messages.SendMessageAsync(reply);
                        }
                    }, 
                    context);
            }
        }
    }
}