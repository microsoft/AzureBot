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
            if (ResourceDialogs == null || (ResourceDialogs.Count != 3))
            {
                lock (resoucelock)
                {
                    if (ResourceDialogs == null)
                    {
                        ResourceDialogs = new List<AzureBotLuisDialog<string>>();
                    }
                    else if (ResourceDialogs.Count != 3)
                    {
                        ResourceDialogs.Clear();
                    }
                    ResourceDialogs.Add((AzureBotLuisDialog<string>)Activator.CreateInstance(typeof(AutomationDialog)));
                    ResourceDialogs.Add((AzureBotLuisDialog<string>)Activator.CreateInstance(typeof(ResourceGroupDialog)));
                    ResourceDialogs.Add((AzureBotLuisDialog<string>)Activator.CreateInstance(typeof(VMDialog)));
                }
            }
        }
    }
}