namespace AzureBot.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    public static class ContextExtensions
    {
        public static string GetSubscription(this TestContext context)
        {
            return context.Properties["Subscription"].ToString();
        }

        public static string GetVirtualMachine(this TestContext context)
        {
            return context.Properties["VirtualMachine"].ToString();
        }

        public static string GetResourceGroup(this TestContext context)
        {
            return context.Properties["ResourceGroup"].ToString();
        }
    }
}
