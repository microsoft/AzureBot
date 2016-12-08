namespace AzureBot.ConsoleConversation
{
    using System;
    using System.Configuration;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using Microsoft.Bot.Connector.DirectLine;
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

            var conversation = await client.Conversations.StartConversationAsync();
            conversationId = conversation.ConversationId;
            new System.Threading.Thread(async () => await ReadBotMessagesAsync()).Start();
            
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
                        Activity userMessage = new Activity
                        {
                            Type = ActivityTypes.Message,
                            From = new ChannelAccount { Id = fromUser },
                            Text = input
                        };

                        await client.Conversations.PostActivityAsync(conversation.ConversationId, userMessage);

                    }
                }
            }
        }

        internal static async Task ReadBotMessagesAsync()
        {
            string watermark = null;
            while (true)
            {
                var activities = await client.Conversations.GetActivitiesAsync(conversationId, watermark);
                watermark = activities?.Watermark;

                var activitiesText = from x in activities.Activities
                                   where x.From.Id == BotId
                                   select x;

                foreach (Activity activity in activitiesText)
                {
                    Console.WriteLine(activity.Text);
                    Console.Write("Command > ");
                }
                await Task.Delay(TimeSpan.FromSeconds(1)).ConfigureAwait(false);
            }
        }
    }
}
