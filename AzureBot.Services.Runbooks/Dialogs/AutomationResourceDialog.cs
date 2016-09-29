using Microsoft.Bot.Builder.Dialogs;
using System.Collections.Generic;
using System.Linq;

namespace AzureBot.Dialogs
{
    public class AutomationResourceDialog : IResourceDialog
    {
        private List<string> keywords = new List<string> { "job", "runbook", "automation", "run", "book" };
        public bool CanHandle(string query)
        {
            return keywords.Any(query.Contains);
        }

        public IDialog<string> Create()
        {
            return new AutomationDialog();
        }
    }
}