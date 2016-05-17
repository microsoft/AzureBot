namespace AzureBot.Azure.Management.ResourceManagement
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Data;
    using Microsoft.Azure.Management.Automation;
    using Microsoft.Azure.Subscriptions;
    using Models;
    using TokenCredentials = Microsoft.Azure.TokenCloudCredentials;

    public class AzureRepository
    {
        public async Task<IEnumerable<Subscription>> ListSubscriptionsAsync(string accessToken)
        {
            var credentials = new TokenCredentials(accessToken);
            IEnumerable<Subscription> subscriptions = new List<Subscription>();

            using (SubscriptionClient client = new SubscriptionClient(credentials))
            {
                var result = await client.Subscriptions.ListAsync();

                subscriptions = result.Subscriptions.Select(sub => new Subscription { SubscriptionId = sub.SubscriptionId, DisplayName = sub.DisplayName }).ToList();
            }

            return subscriptions;
        }

        public async Task<IEnumerable<VirtualMachine>> ListVirtualMachinesAsync(string subscriptionId)
        {
            return await Task.FromResult(MockData.GetVirtualMachines().Where(p => p.SubscriptionId == subscriptionId));
        }

        public async Task<IEnumerable<AutomationAccount>> ListAutomationAccountsAsync(string accessToken, string subscriptionId)
        {
            var credentials = new TokenCredentials(subscriptionId, accessToken);
            IEnumerable<AutomationAccount> automationAccounts = new List<AutomationAccount>();

            using (var automationClient = new AutomationManagementClient(credentials))
            {
                var automationAccountsResult = await automationClient.AutomationAccounts.ListAsync(null);

                automationAccounts = await Task.WhenAll(
                    automationAccountsResult.AutomationAccounts.Select(
                        async account => new AutomationAccount
                        {
                            AutomationAccountId = account.Id,
                            AutomationAccountName = account.Name,
                            RunBooks = await this.ListAutomationRunBooks(accessToken, subscriptionId, GetResourceGroup(account.Id), account.Name)
                        }).ToList());
            }

            return automationAccounts;
        }

        public async Task<IEnumerable<RunBook>> ListAutomationRunBooks(string accessToken, string subscriptionId, string resourceGroupName, string automationAccountName)
        {
            var credentials = new TokenCredentials(subscriptionId, accessToken);
            IEnumerable<RunBook> automationRunBooks = new List<RunBook>();

            using (var automationClient = new AutomationManagementClient(credentials))
            {
                var automationRunBooksResult = await automationClient.Runbooks.ListAsync(resourceGroupName, automationAccountName);

                automationRunBooks = automationRunBooksResult.Runbooks.Select(
                    runBook => new RunBook { RunBookId = runBook.Id, RunBookName = runBook.Name }).ToList();
            }

            return automationRunBooks;
        }

        public async Task<bool> StartVirtualMachineAsync(string subscriptionId, string virtualMachineName)
        {
            return await Task.FromResult(true);
        }

        public async Task<bool> StopVirtualMachineAsync(string subscriptionId, string virtualMachineName)
        {
            return await Task.FromResult(true);
        }

        public async Task<bool> RunRunBookAsync(string subscriptionId, string automationAccountName, string runBookName)
        {
            return await Task.FromResult(true);
        }

        private static string GetResourceGroup(string id)
        {
            var segments = id.Split('/');
            var resourceGroupName = segments.SkipWhile(segment => segment != "resourceGroups").ElementAtOrDefault(1);
            return resourceGroupName;
        }
    }
}