namespace AzureBot.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Bot.Connector.DirectLine;
    using System.Threading;

    public class BotHelper : IDisposable 
    {
        private string watermark;
        private string microsoftAppId;
        private string fromUser;
        private string botId;
        private DirectLineClient directLineClient;
        private Conversation conversation;

        private bool disposed = false;

        public BotHelper(string directLineToken, string microsoftAppId, string fromUser, string BotId)
        {
            this.microsoftAppId = microsoftAppId;
            this.fromUser = fromUser;
            botId = BotId;
            directLineClient = new DirectLineClient(directLineToken);
            conversation = directLineClient.Conversations.StartConversation();
        }

        public async Task<string> SendMessage(string msg)
        {
            await SendMessageNoReply(msg);
            return await LastMessageFromBot();
        }

        public async Task SendMessageNoReply(string msg)
        {
            await directLineClient.Conversations.PostActivityAsync(conversation.ConversationId, MakeActivity(msg), CancellationToken.None);
        }

        private Activity MakeActivity(string msg)
        {
            // Passing in a value in From makes the bot 'remember' that it's the same user
            // and loads the user context that will have been set up previously outside the tests
            return new Activity()
            {
                Type = ActivityTypes.Message,
                From = new ChannelAccount { Id = fromUser },
                Text = msg
            };
        }

        public async Task<string> LastMessageFromBot()
        {
            var botMessages = await AllBotMessagesSinceWatermark();
            return botMessages.Last();
        }

        public async Task WaitForLongRunningOperations(Action<IList<string>> resultHandler, int operationsToWait, int delayBetweenPoolingInSeconds = 4)
        {
            var currentWatermark = watermark;
            var messages = await AllBotMessagesSinceWatermark(currentWatermark).ConfigureAwait(false);
            var iterations = 0;
            var maxIterations = (5 * 60) / delayBetweenPoolingInSeconds;

            while (iterations < maxIterations && messages.Count < operationsToWait)
            {
                await Task.Delay(TimeSpan.FromSeconds(delayBetweenPoolingInSeconds)).ConfigureAwait(false);
                messages = await AllBotMessagesSinceWatermark(currentWatermark);
                iterations++;
            }

            resultHandler(messages);
        }

        private async Task<IList<string>> AllBotMessagesSinceWatermark(string specificWatermark = null)
        {
            var messages = await AllMessagesSinceWatermark(specificWatermark);
            var messagesText = from x in messages
                               where x.From.Id == botId
                               select x.Text.Trim();
            return messagesText.ToList();
        }

        private async Task<IList<Activity>> AllMessagesSinceWatermark(string specificWatermark = null)
        {
            specificWatermark = string.IsNullOrEmpty(specificWatermark) ? watermark : specificWatermark;
            ActivitySet messageSet = await directLineClient.Conversations.GetActivitiesAsync(conversation.ConversationId, specificWatermark);
            watermark = messageSet?.Watermark;
            return messageSet.Activities;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }

            if (disposing)
            {
                directLineClient.Dispose();
            }

            disposed = true;
        }
    }
}
