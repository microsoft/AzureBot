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

            var reply = botHelper.SendMessage("select subscription").Result;
            string expected = "Please select the subscription";
            Assert.IsTrue(reply.StartsWith(expected));

            reply = botHelper.SendMessage("DevOps02-Internal").Result;
            expected = "Setting DevOps02-Internal";
            Assert.IsTrue(reply.StartsWith(expected));
        }

        // Will run after all the tests have finished
        [AssemblyCleanup]
        public static void CleanUp()
        {
            var expected = "You are trying to stop the following virtual machines";

            var reply = botHelper.SendMessage("stop all vms").Result;

            Assert.IsTrue(reply.StartsWith(expected));

            reply = botHelper.SendMessage("Yes").Result;

            if (botHelper != null)
            {
                botHelper.Dispose();
            }
        }
    }
}
