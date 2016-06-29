namespace AzureBot.Tests
{
    using System;

    internal class BotTestCase
    {
        public string Action { get; internal set; }

        public string ExpectedReply { get; internal set; }

        public Func<string, string, string> ErrorMessageHandler { get; internal set; }

        public Action<string> Verified { get; internal set; }
    }
}
