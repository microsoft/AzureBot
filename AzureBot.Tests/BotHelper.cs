namespace AzureBot.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Bot.Connector.DirectLine;
    using Microsoft.Bot.Connector.DirectLine.Models;

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
            this.botId = BotId;
            this.directLineClient = new DirectLineClient(directLineToken);
            this.conversation = this.directLineClient.Conversations.NewConversation();
        }

        public async Task<string> SendMessage(string msg)
        {
            await this.SendMessageNoReply(msg);
            return await this.LastMessageFromBot();
        }

        public async Task SendMessageNoReply(string msg)
        {
            // Passing in a value in FromProperty makes the bot 'remember' that it's the same user
            // and loads the user context that will have been set up previously outside the tests
            Message message = new Message { FromProperty = this.fromUser, Text = msg };
            await this.directLineClient.Conversations.PostMessageAsync(this.conversation.ConversationId, message);
        }

        public async Task<string> LastMessageFromBot()
        {
            var botMessages = await this.AllBotMessagesSinceWatermark();
            return botMessages.Last();
        }

        public async Task WaitForLongRunningOperations(Action<IList<string>> resultHandler, int operationsToWait, int delayBetweenPoolingInSeconds = 4)
        {
            var currentWatermark = this.watermark;
            var messages = await this.AllBotMessagesSinceWatermark(currentWatermark).ConfigureAwait(false);
            var iterations = 0;
            var maxIterations = (5 * 60) / delayBetweenPoolingInSeconds;

            while (iterations < maxIterations && messages.Count < operationsToWait)
            {
                await Task.Delay(TimeSpan.FromSeconds(delayBetweenPoolingInSeconds)).ConfigureAwait(false);
                messages = await this.AllBotMessagesSinceWatermark(currentWatermark);
                iterations++;
            }

            resultHandler(messages);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this.disposed)
            {
                return;
            }

            if (disposing)
            {
                this.directLineClient.Dispose();
            }

            this.disposed = true;
        }

        private async Task<IList<string>> AllBotMessagesSinceWatermark(string specificWatermark = null)
        {
            var messages = await this.AllMessagesSinceWatermark(specificWatermark);
            var messagesText = from x in messages
                               where x.FromProperty == this.botId
                               select x.Text;
            return messagesText.ToList();
        }

        private async Task<IList<Message>> AllMessagesSinceWatermark(string specificWatermark = null)
        {
            specificWatermark = string.IsNullOrEmpty(specificWatermark) ? this.watermark : specificWatermark;
            MessageSet messageSet = await this.directLineClient.Conversations.GetMessagesAsync(this.conversation.ConversationId, specificWatermark);
            this.watermark = messageSet?.Watermark;
            return messageSet.Messages;
        }
    }
}
