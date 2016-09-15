using Microsoft.Bot.Builder.Dialogs;
using System.Collections.Generic;
using System.Linq;

namespace AzureBot.Dialogs
{
    public class VmResourceDialog:IResourceDialog
    {
        private List<string> keywords = new List<string> { "vm", "virtual machine", "machine", "vms", "machines" };
        public bool CanHandle(string query)
        {
            return keywords.Any(query.Contains);
        }

        public IDialog<string> Create()
        {
            return new VMDialog();            
        }
    }
}