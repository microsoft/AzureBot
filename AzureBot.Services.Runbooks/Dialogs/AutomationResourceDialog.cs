using Microsoft.Bot.Builder.Dialogs;

namespace AzureBot.Dialogs
{
    public class AutomationResourceDialog:IResourceDialog
    {
        public bool CanHandle(string query)
        {
            return query.ToLowerInvariant().Contains("job") | query.ToLowerInvariant().Contains("runbook");
        }

        public IDialog<string> Create()
        {
            return new AutomationDialog();
        }
    }
}