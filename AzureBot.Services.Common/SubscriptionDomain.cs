using Microsoft.Azure;
using Microsoft.Azure.Subscriptions;
using Microsoft.Rest;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AzureBot.Domain
{
    public static class Subscription
    {
        public static async Task<IEnumerable<Models.Subscription>> ListSubscriptionsAsync(string accessToken)
        {
            var credentials = new TokenCloudCredentials(accessToken);

            using (SubscriptionClient client = new SubscriptionClient(credentials))
            {
                var subscriptionsResult = await client.Subscriptions.ListAsync().ConfigureAwait(false);
                var subscriptions = subscriptionsResult.Subscriptions
                                    .OrderBy(x => x.DisplayName)
                                    .Select(sub => new Models.Subscription {SubscriptionId = sub.SubscriptionId,
                                                                            DisplayName = sub.DisplayName })
                                    .ToList();
                return subscriptions;
            }
        }

        public static async Task<Models.Subscription> GetSubscription(string accessToken, string subscriptionId)
        {
            var credentials = new TokenCloudCredentials(accessToken);

            using (SubscriptionClient client = new SubscriptionClient(credentials))
            {
                var subscriptionsResult = await client.Subscriptions.GetAsync(subscriptionId, CancellationToken.None);
                return new Models.Subscription
                {
                    SubscriptionId = subscriptionsResult.Subscription.SubscriptionId,
                    DisplayName = subscriptionsResult.Subscription.DisplayName
                };
            }
        }

        public static string GetResourceGroup(string id)
        {
            var segments = id.Split('/');
            var resourceGroupName = segments.SkipWhile(segment => segment != "resourceGroups").ElementAtOrDefault(1);
            return resourceGroupName;
        }
    }
}
