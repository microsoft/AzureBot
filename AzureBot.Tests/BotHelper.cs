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
        private string appId;
        private string fromUser;
        private DirectLineClient directLineClient;
        private Conversation conversation;

        private bool disposed = false;

        public BotHelper(string directLineToken, string appId, string fromUser)
        {
            this.appId = appId;
            this.fromUser = fromUser;
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

        public async Task WaitForLongRunningOperation(Action<string> resultHandler, int delayBetweenPoolingInSeconds = 5)
        {
            var messages = await this.AllBotMessagesSinceWatermark().ConfigureAwait(false);

            while (!messages.Any())
            {
                await Task.Delay(TimeSpan.FromSeconds(delayBetweenPoolingInSeconds)).ConfigureAwait(false);
                messages = await this.AllBotMessagesSinceWatermark();
            }

            resultHandler(messages.Last());
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

        private async Task<IList<string>> AllBotMessagesSinceWatermark()
        {
            var messages = await this.AllMessagesSinceWatermark();
            var messagesText = from x in messages
                               where x.FromProperty == this.appId
                               select x.Text;
            return messagesText.ToList();
        }

        private async Task<IList<Message>> AllMessagesSinceWatermark()
        {
            MessageSet messageSet = await this.directLineClient.Conversations.GetMessagesAsync(this.conversation.ConversationId, this.watermark);
            this.watermark = messageSet?.Watermark;
            return messageSet.Messages;
        }
    }
}
