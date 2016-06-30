namespace AzureBot.Tests
{
    using System;

    internal class BotTestCase
    {
        public BotTestCase()
        {
            this.ErrorMessageHandler = DefaultErrorMessageHandler;
        }

        public string Action { get; internal set; }

        public string ExpectedReply { get; internal set; }

        public Func<string, string, string, string> ErrorMessageHandler { get; internal set; }

        public Action<string> Verified { get; internal set; }

        private static string DefaultErrorMessageHandler(string action, string expectedReply, string receivedReply)
        {
            return $"'{action}' received reply '{receivedReply}' that doesn't contain the expected message: '{expectedReply}'";
        }
    }
}
