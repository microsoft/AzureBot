using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using AzureBot.Azure.Management.Models;
using AzureBot.Azure.Management.ResourceManagement;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.FormFlow;
using Microsoft.Bot.Builder.FormFlow.Advanced;

namespace AzureBot
{
    public class Forms
    {
        public static IForm<Subscription> BuildSubscriptionForm()
        {
            return new FormBuilder<Subscription>()
                .Message("Select the subscription you want to work with")
                .Field(new FieldReflector<Subscription>(nameof(Subscription.SubscriptionId))
                .SetType(null)
                .SetDefine(async (state, field) =>
               {
                   var subscriptions = await (new AzureRepository().ListSubscriptionsAsync());
                   foreach (var sub in subscriptions)
                   {
                       field.AddDescription(sub.SubscriptionId, sub.DisplayName)
                           .AddTerms(sub.SubscriptionId, sub.DisplayName);
                   }

                   return true;
               }))
               .Build();
        }
    }
}