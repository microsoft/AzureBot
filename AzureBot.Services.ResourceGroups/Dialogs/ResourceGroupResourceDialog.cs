using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureBot.Dialogs
{
    class ResourceGroupResourceDialog: IResourceDialog
    {
        private List<string> keywords = new List<string> { "rg", "resource group", "resourcegroup"};
        public bool CanHandle(string query)
        {
            return keywords.Any(query.Contains);
        }
        public IDialog<string> Create()
        {
            return new ResourceGroupDialog();
        }
    }
}
