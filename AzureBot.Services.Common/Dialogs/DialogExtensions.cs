namespace AzureBot.Dialogs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Autofac;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.Dialogs.Internals;
    using Microsoft.Bot.Builder.Luis.Models;
    using Microsoft.Bot.Connector;
    public static class DialogExtensions
    {
        public static void NotifyLongRunningOperation<T>(this Task<T> operation, IDialogContext context, Func<T, string> handler)
        {
            operation.ContinueWith(
                async (t, ctx) =>
                {
                    var messageText = handler(t.Result);
                    await NotifyUser((IDialogContext)ctx, messageText);
                },
                context);
        }

        public static void NotifyLongRunningOperation<T, Y>(this Task<T> operation, IDialogContext context, Func<T, Y, string> handler, Y item)
        {
            operation.ContinueWith(
                async (t, ctx) =>
                {
                    var messageText = handler(t.Result, item);
                    await NotifyUser((IDialogContext)ctx, messageText);
                },
                context);
        }

        public static void NotifyLongRunningOperation<T>(this Task<T> operation, IDialogContext context, Func<T, IDialogContext, string> handler)
        {
            operation.ContinueWith(
                async (t, ctx) =>
                {
                    var messageText = handler(t.Result, (IDialogContext)ctx);
                    await NotifyUser((IDialogContext)ctx, messageText);
                },
                context);
        }

        public static string GetEntityOriginalText(this EntityRecommendation recommendation, string query)
        {
            if (recommendation.StartIndex.HasValue && recommendation.EndIndex.HasValue)
            {
                return query.Substring(recommendation.StartIndex.Value, recommendation.EndIndex.Value - recommendation.StartIndex.Value + 1);
            }

            return null;
        }

        public static async Task NotifyUser(this IDialogContext context, string messageText)
        {
            if (!string.IsNullOrEmpty(messageText))
            {
                string serviceUrl = context.PrivateConversationData.Get<string>("ServiceUrl");
                var connector = new ConnectorClient(new Uri(serviceUrl));
                var reply = context.MakeMessage();
                reply.Text = messageText;
                await connector.Conversations.ReplyToActivityAsync((Activity)reply);
            }
        }
    }
}