using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Bot.Builder.Dialogs;
using System.Threading.Tasks;

namespace AzureBot.Dialogs
{
    public class DialogFactory
    {
        private static object resoucelock= new object();
        private static List<AzureBotLuisDialog<string>> ResourceDialogs { get; set; }

        public async Task<AzureBotLuisDialog<string>> Create(string query)
        {
            query = query.ToLowerInvariant();
            EnsureResourceDialogs();
            foreach (var resourceDialog in ResourceDialogs)
            {
                if (await resourceDialog.CanHandle(query))
                {
                    return resourceDialog;
                }
            }
            return null; 
        }

        private void EnsureResourceDialogs()
        {
            if (ResourceDialogs == null || !ResourceDialogs.Any())
            {
                lock (resoucelock)
                {
                    if (ResourceDialogs == null || !ResourceDialogs.Any())
                    {
                        var type = typeof(AzureBotLuisDialog<string>);
                        var assemblies=AppDomain.CurrentDomain.GetAssemblies().Where(a=>a.FullName.StartsWith("AzureBot.Services")).ToList();
                        ResourceDialogs = assemblies.SelectMany(a => a.GetTypes()).Where(a => type.IsAssignableFrom(a)).Select(a=> (AzureBotLuisDialog<string>)Activator.CreateInstance(a)).ToList();
                    }
                }
            }
        }
    }
}