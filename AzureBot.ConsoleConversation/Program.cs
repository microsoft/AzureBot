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

        internal static void Main(string[] args)
        {
            StartBotConversation().Wait();
        }

        internal static async Task StartBotConversation()
        {
            DirectLineClient client = new DirectLineClient(directLineToken);

            string watermark = null;

            var conversation = await client.Conversations.NewConversationAsync();
            
            //After authenticating using this app, then the tests in the Tests project should work 
            //as long as the FromUser setting is the same between them
            while (true)
            {
                Console.Write("Command > ");
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

                        Debug.WriteLine($"Sending Message: {input}");

                        await client.Conversations.PostMessageAsync(conversation.ConversationId, userMessage);
                        var messages = await client.Conversations.GetMessagesAsync(conversation.ConversationId, watermark);
                        watermark = messages?.Watermark;

                        Debug.WriteLine($"Received {messages.Messages.Count}");
             
                        var messagesText = from x in messages.Messages
                                           where x.FromProperty == BotId
                                           select x;

                        foreach (Message message in messagesText)
                        {
                            Debug.WriteLine(message.FromProperty);
                            Debug.WriteLine(message.Text);
                            Debug.WriteLine("------------------------------");
                            Console.WriteLine(message.Text);
                        }
                    }
                }
            }
        }
   }
}
