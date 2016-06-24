using Microsoft.Bot.Connector.DirectLine;
using Microsoft.Bot.Connector.DirectLine.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureBot.ConsoleConversation
{
    class Program
    {
        public static string ConversationId = ConfigurationManager.AppSettings["ConversationId"];
        public static string DirectLineToken = ConfigurationManager.AppSettings["DirectLineToken"];
        public static string AppId = ConfigurationManager.AppSettings["AppId"];
        public static string FromUser = ConfigurationManager.AppSettings["FromUser"];
        static void Main(string[] args)
        {
            StartBotConversation().Wait();
        }

        static async Task StartBotConversation()
        {
            DirectLineClient client = new DirectLineClient(DirectLineToken);
            string strWatermark = null;
            MessageSet msgs = await client.Conversations.GetMessagesAsync(ConversationId, strWatermark);
            strWatermark = msgs?.Watermark;
            while (true)
            {
                Console.Write("Command>");
                string strInput = Console.ReadLine().Trim();

                if (strInput.ToLower() == "exit")
                    break;
                else
                {

                    if (strInput.Length > 0)
                    {
                        Message aMsg = new Message
                        {
                            FromProperty = FromUser,
                            Text = strInput
                        };

                        Debug.WriteLine($"Sending Message: {strInput}");
                        await client.Conversations.PostMessageAsync(ConversationId, aMsg);
                        msgs = await client.Conversations.GetMessagesAsync(ConversationId, strWatermark);
                        strWatermark = msgs?.Watermark;

                        Debug.WriteLine($"Received {msgs.Messages.Count}");
                        foreach (Message m in msgs.Messages)
                        {
                            Debug.WriteLine(m.FromProperty);
                            Debug.WriteLine(m.Text);
                            Debug.WriteLine("------------------------------");
                            if ("azurebot" == m.FromProperty)
                                Console.WriteLine(m.Text);
                        }
                    }
                }
            }
        }
    }
}
