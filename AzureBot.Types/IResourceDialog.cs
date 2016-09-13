using Microsoft.Bot.Builder.Dialogs;

namespace AzureBot.Dialogs
{
    public interface IResourceDialog
    {
        bool CanHandle(string query);
        IDialog<string> Create();
    }
}