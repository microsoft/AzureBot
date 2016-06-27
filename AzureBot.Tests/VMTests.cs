namespace AzureBot.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Tests all VM commands
    /// </summary>
    [TestClass]
    public class VMTests
    {
        [TestMethod]
        public void CanListVMs()
        {
            string expected = "Available VMs are";
            General.BotHelper.SendMessage("list vms");
            string lastMessage = General.BotHelper.LastMessageFromBot().Result;
            Assert.IsTrue(lastMessage.StartsWith(expected));
        }
    }
}
