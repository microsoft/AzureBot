namespace AzureBot.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class General
    {
        private static BotHelper botHelper;

        internal static BotHelper BotHelper
        {
            get { return botHelper; }
        }

        // Will run once before all of the tests in the project. We start assuming the user is already logged in to Azure,
        // which should  be done separately via the AzureBot.ConsoleConversation or some other means. 
        [AssemblyInitialize]
        public static void SetUp(TestContext context)
        {
            string directLineToken = context.Properties["DirectLineToken"].ToString();
            string appId = context.Properties["AppId"].ToString();
            string fromUser = context.Properties["FromUser"].ToString();
            botHelper = new BotHelper(directLineToken, appId, fromUser);

            botHelper.SendMessage("select subscription");
            string lastMessage = botHelper.LastMessageFromBot().Result;
            string expected = "Please select the subscription";
            Assert.IsTrue(lastMessage.StartsWith(expected));

            botHelper.SendMessage("DevOps02-Internal");
            lastMessage = botHelper.LastMessageFromBot().Result;
            expected = "Setting DevOps02-Internal";
            Assert.IsTrue(lastMessage.StartsWith(expected));
        }

        // Will run after all the tests have finished
        [AssemblyCleanup]
        public static void CleanUp()
        {
            if (botHelper != null)
            {
                botHelper.Dispose();
            }
        }
    }
}
