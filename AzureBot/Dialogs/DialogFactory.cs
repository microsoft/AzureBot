using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Bot.Builder.Dialogs;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Bot.Builder.Luis;
using System.Configuration;

namespace AzureBot.Dialogs
{
    public class DialogFactory
    {
        private static object resoucelock= new object();
        private static List<AzureBotLuisDialog<string>> ResourceDialogs { get; set; }

        public async Task<AzureBotLuisDialog<string>> Create(string query, CancellationToken cancellationToken)
        {
            query = query.ToLowerInvariant();
            EnsureResourceDialogs();
            foreach (var resourceDialog in ResourceDialogs)
            {
                if (await resourceDialog.CanHandle(query, cancellationToken))
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
                    
                    ResourceDialogs.Add(new AutomationDialog(new LuisService(new LuisModelAttribute(ConfigurationManager.AppSettings["AutomationDialog.AppId"],
                                                                                                   ConfigurationManager.AppSettings["LuisAPIKey"]))));
                    ResourceDialogs.Add(new ResourceGroupDialog(new LuisService(new LuisModelAttribute(ConfigurationManager.AppSettings["ResourceGroupDialog.AppId"],
                                                                                                   ConfigurationManager.AppSettings["LuisAPIKey"]))));
                    ResourceDialogs.Add(new VMDialog(new LuisService(new LuisModelAttribute(ConfigurationManager.AppSettings["VMDialog.AppId"],
                                                                                                   ConfigurationManager.AppSettings["LuisAPIKey"])))); ;
                }
            }
        }
    }
}