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

        public static EntityRecommendation ResolveEntity<T>(this EntityRecommendation recommendation, IEnumerable<T> entitySet, Func<T, string> selector, string originalText)
        {
            var matchingName = entitySet.Select(selector).FirstOrDefault(p => p == originalText);
            if (matchingName != null)
            {
                return new EntityRecommendation(
                    role: recommendation.Role,
                    entity: matchingName,
                    type: recommendation.Type,
                    startIndex: recommendation.StartIndex,
                    endIndex: recommendation.EndIndex,
                    score: recommendation.Score,
                    resolution: recommendation.Resolution);
            }

            return null;
        }

        public static string GetEntityOriginalText(this EntityRecommendation recommendation, string query)
        {
            if (recommendation.StartIndex.HasValue && recommendation.EndIndex.HasValue)
            {
                return query.Substring(recommendation.StartIndex.Value, recommendation.EndIndex.Value - recommendation.StartIndex.Value + 1);
            }

            return null;
        }

        private static async Task NotifyUser(IDialogContext context, string messageText)
        {
            var reply = context.MakeMessage();
            reply.Text = messageText;

            using (var scope = DialogModule.BeginLifetimeScope(Conversation.Container, reply))
            {
                var client = scope.Resolve<IConnectorClient>();
                await client.Messages.SendMessageAsync(reply);
            }
        }
    }
}