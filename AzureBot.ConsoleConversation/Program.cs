namespace AzureBot.ConsoleConversation
{
    using System;
    using System.Configuration;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using Microsoft.Bot.Connector.DirectLine;
    using Microsoft.Bot.Connector.DirectLine.Models;
    using System.Linq;
    internal class Program
    {
        private static string directLineToken = ConfigurationManager.AppSettings["DirectLineToken"];
        private static string microsoftAppId = ConfigurationManager.AppSettings["MicrosoftAppId"];
        private static string fromUser = ConfigurationManager.AppSettings["FromUser"];
        private static string BotId = ConfigurationManager.AppSettings["BotId"];
        private static DirectLineClient client = new DirectLineClient(directLineToken);
        private static string conversationId;
        internal static void Main(string[] args)
        {
            StartBotConversation().Wait();
        }

        internal static async Task StartBotConversation()
        {

            var conversation = await client.Conversations.NewConversationAsync();
            conversationId = conversation.ConversationId;
            var t = new System.Threading.Thread(async () => await ReadBotMessagesAsync());
            t.Start();
            t.Join();

            //After authenticating using this app, then the tests in the Tests project should work 
            //as long as the FromUser setting is the same between them

            Console.Write("Command > ");
            while (true)
            {
                string input = Console.ReadLine().Trim();

                if (input.ToLower() == "exit")
                {
                    break;
                }
                else
                {
                    if (input.Length > 0)
                    {
                        Message userMessage = new Message
                        {
                            FromProperty = fromUser,
                            Text = input
                        };

                        await client.Conversations.PostMessageAsync(conversation.ConversationId, userMessage);

                    }
                }
            }
        }

        internal static async Task ReadBotMessagesAsync()
        {
            string watermark = null;
            while (true)
            {
                var messages = await client.Conversations.GetMessagesAsync(conversationId, watermark);
                watermark = messages?.Watermark;

                var messagesText = from x in messages.Messages
                                   where x.FromProperty == BotId
                                   select x;

                foreach (Message message in messagesText)
                {
                    Console.WriteLine(message.Text);
                    Console.Write("Command > ");
                }
                await Task.Delay(TimeSpan.FromSeconds(1)).ConfigureAwait(false);
            }
        }
    }
}
