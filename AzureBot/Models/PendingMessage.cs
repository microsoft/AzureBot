namespace AzureBot.Models
{
    using System;
    using Microsoft.Bot.Connector;

    /// <summary>
    /// The pending message that is written to <see cref="Microsoft.Bot.Builder.Dialogs.Internals.IBotData.PerUserInConversationData"/>
    /// by <see cref="AzureBotDialog"/> to indicate that it is waiting for the auth callback. This pending message is then used 
    /// to send the reply back to the user on the right channel upon completion of the login flow.
    /// </summary>
    [Serializable]
    public sealed class PendingMessage
    {
        public string userId;
        public string userAddress;
        public string userChannelId;

        public string botId;
        public string botAddress;
        public string botChannelId;

        public string conversationId;

        public string Text;

        public PendingMessage()
        {
        }

        public PendingMessage(Message msg)
        {
            this.Text = msg.Text;
            userId = msg.From?.Id;
            userAddress = msg.From?.Address;
            userChannelId = msg.From?.ChannelId;
            botId = msg.To?.Id;
            botAddress = msg.To?.Address;
            botChannelId = msg.To?.ChannelId;
            conversationId = msg.ConversationId;
        }

        public Message GetMessage()
        {
            return new Message
            {
                Id = Guid.NewGuid().ToString(),
                To = new ChannelAccount
                {
                    Id = botId,
                    IsBot = true,
                    Address = botAddress,
                    ChannelId = botChannelId
                },
                ConversationId = conversationId,
                From = new ChannelAccount
                {
                    Id = userId,
                    IsBot = false,
                    Address = userAddress,
                    ChannelId = userChannelId
                }
            };
        }
    }
}