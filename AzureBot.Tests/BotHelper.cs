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

        public void SendMessage(string msg)
        {
            // Passing in a value in FromProperty makes the bot 'remember' that it's the same user
            // and loads the user context that will have been set up previously outside the tests
            Message message = new Message { FromProperty = this.fromUser, Text = msg };
            this.directLineClient.Conversations.PostMessage(this.conversation.ConversationId, message);
        }

        public async Task<string> LastMessageFromBot()
        {
            var botMessages = await this.AllBotMessagesSinceWatermark();
            return botMessages.Last();
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
