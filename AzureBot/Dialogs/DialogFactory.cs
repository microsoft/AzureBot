using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Bot.Builder.Dialogs;

namespace AzureBot.Dialogs
{
    public class DialogFactory
    {
        private static object resoucelock= new object();
        private static List<IResourceDialog> ResourceDialogs { get; set; }

        public IDialog<string> Create(string query)
        {
            EnsureResourceDialogs();
            foreach (var resourceDialog in ResourceDialogs)
            {
                if (resourceDialog.CanHandle(query))
                {
                    return resourceDialog.Create();
                }
            }
            return null; 
        }

        private void EnsureResourceDialogs()
        {
            if (!ResourceDialogs.Any())
            {
                lock (resoucelock)
                {
                    if (!ResourceDialogs.Any())
                    {
                        var type = typeof(IResourceDialog);
                        var assemblies=AppDomain.CurrentDomain.GetAssemblies().Where(a=>a.FullName.StartsWith("AzureBot.Services")).ToList();
                        ResourceDialogs = assemblies.SelectMany(a => a.GetTypes()).Where(a => type.IsAssignableFrom(a)).Select(a=> (IResourceDialog)Activator.CreateInstance(a)).ToList();
                    }
                }
            }
        }
    }
}