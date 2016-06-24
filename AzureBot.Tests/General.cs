using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Configuration;
using System.Threading.Tasks;

namespace AzureBot.Tests
{
    [TestClass]
    public class General
    {
        public static BotHelper botHelper;

        [AssemblyInitialize]
        //Will run once before all of the tests in the project. We start assuming the user is already logged in to Azure,
        //which should  be done separately via the AzureBot.ConsoleConversation or some other means. 
        public static void SetUp(TestContext context)
        {
            string DirectLineToken = context.Properties["DirectLineToken"].ToString();
            string AppId = context.Properties["AppId"].ToString();
            string FromUser = context.Properties["FromUser"].ToString();
            botHelper = new BotHelper(DirectLineToken, AppId, FromUser);

            botHelper.SendMessage("select subscription");
            string lastMessage = botHelper.LastMessageFromBot().Result;
            string expected = "Please select the subscription";
            Assert.IsTrue(lastMessage.StartsWith(expected));

            botHelper.SendMessage("DevOps02-Internal");
            lastMessage = botHelper.LastMessageFromBot().Result;
            expected = "Setting DevOps02-Internal";
            Assert.IsTrue(lastMessage.StartsWith(expected));
        }

        [AssemblyCleanup]
        //Will run after all the tests have finished
        public static void CleanUp()
        {
            if (botHelper != null)
            {
                botHelper.Dispose();
            }
        }
    }
}
