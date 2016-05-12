namespace AzureBot.Azure.Management.ResourceManagement
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AzureBot.Azure.Management.Models;
    using Data;
    public class AzureRepository
    {
        public Task<IEnumerable<Subscription>> ListSubscriptionsAsync()
        {
            return Task.FromResult(MockData.GetSubscriptions());
        }
    }
}
