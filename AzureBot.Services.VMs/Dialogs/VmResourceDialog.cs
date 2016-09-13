using Microsoft.Bot.Builder.Dialogs;

namespace AzureBot.Dialogs
{
    public class VmResourceDialog:IResourceDialog
    {
        public bool CanHandle(string query)
        {
            return query.ToLowerInvariant().Contains("vm");
        }

        public IDialog<string> Create()
        {
            return new VMDialog();            
        }
    }
}