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

        public static string GetRunbookWithDescription(this TestContext context)
        {
            return context.Properties["RunbookWithDescription"].ToString();
        }

        public static string GetRunbookDescription(this TestContext context)
        {
            return context.Properties["RunbookDescription"].ToString();
        }

        public static string GetRunbookWithoutDescription(this TestContext context)
        {
            return context.Properties["RunbookWithoutDescription"].ToString();
        }

        public static string GetRunbookInMultipleAutomationAccounts(this TestContext context)
        {
            return context.Properties["RunbookInMultipleAutomationAccounts"].ToString();
        }

        public static string GetRunbookNotPublished(this TestContext context)
        {
            return context.Properties["RunbookNotPublished"].ToString();
        }

        public static string GetAutomationAcccount(this TestContext context)
        {
            return context.Properties["AutomationAccount"].ToString();
        }
    }
}
