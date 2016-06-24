using Microsoft.Bot.Connector.DirectLine;
using Microsoft.Bot.Connector.DirectLine.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;

namespace AzureBot.Tests
{
    public class BotHelper :IDisposable 
    {
        private static string _watermark;
        private static string _directLineToken;
        private static string _appId;
        private static string _fromUser;
        private static DirectLineClient botClient;
        private static Conversation conv;

        public BotHelper(string DirectLineToken, string AppId, string FromUser)
        {
            _directLineToken = DirectLineToken;
            _appId = AppId;
            _fromUser = FromUser;
            botClient = new DirectLineClient(DirectLineToken);
            conv = botClient.Conversations.NewConversation();
        }

        public async Task<string> SendNewMessageAndGetBotReply(string msg)
        {
            Message botMsg = new Message { Text = msg };
            Conversation newconv = await botClient.Conversations.NewConversationAsync();
            //Making sure the message is sent to the bot before continuing
            await botClient.Conversations.PostMessageAsync(newconv.ConversationId, botMsg);
            MessageSet msgs = await botClient.Conversations.GetMessagesAsync(newconv.ConversationId, null);
            return msgs.Messages.Last().Text;
        }

        public void SendMessage(string msg)
        {
            //Passing in a value in FromProperty makes the bot 'remember' that it's the same user
            //and loads the user context that will have been set up previously outside the tests
            Message botMsg = new Message { FromProperty=_fromUser, Text = msg };
            botClient.Conversations.PostMessage(conv.ConversationId, botMsg);
        }

        public async Task<string> LastMessageFromBot()
        {
            var botMessages = await AllBotMessagesSinceWatermark();
            return botMessages.Last();
        }

        private async Task<IList<Message>> _allMessagesInConversation()
        {
            MessageSet msgs = await botClient.Conversations.GetMessagesAsync(conv.ConversationId, null);
            _watermark = msgs?.Watermark;   
            return msgs.Messages;
        }

        public async Task<IList<string>> AllMessagesInConversation()
        {
            var messages = await _allMessagesInConversation();
            var q = from x in messages select x.Text;
            return q.ToList();
        }

        public async Task<IList<string>> AllBotMessagesInConversation()
        {
            var messages = await _allMessagesInConversation();
            var q = from x in messages
                    where x.FromProperty == _appId
                    select x.Text;
            return q.ToList();
        }

        private async Task<IList<Message>> _allMessagesSinceWatermark()
        {
            MessageSet msgs = await botClient.Conversations.GetMessagesAsync(conv.ConversationId, _watermark);
            _watermark = msgs?.Watermark;
            return msgs.Messages;
        }

        public async Task<IList<string>> AllMessagesSinceWatermark()
        {
            var messages = await _allMessagesSinceWatermark();
            var q = from x in messages select x.Text;
            return q.ToList();
        }

        public async Task<IList<string>> AllBotMessagesSinceWatermark()
        {
            var messages = await _allMessagesSinceWatermark();
            var q = from x in messages
                    where x.FromProperty == _appId
                    select x.Text;
            return q.ToList();
        }

        public void Dispose()
        {
            if (botClient != null)
                botClient.Dispose();
        }
    }
}
