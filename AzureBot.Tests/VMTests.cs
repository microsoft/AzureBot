using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace AzureBot.Tests
{
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
            General.botHelper.SendMessage("list vms");
            string lastMessage = General.botHelper.LastMessageFromBot().Result;
            Assert.IsTrue(lastMessage.StartsWith(expected));
        }
    }
}
