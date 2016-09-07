using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureBot.Dialogs
{
    public interface IAzureBotDialog<T> : IDialog<T>
    {
        Task UnableToHandleUserMessage(IDialogContext context, IAwaitable<IMessageActivity> item);

        Task MessageReceived(IDialogContext context, IAwaitable<IMessageActivity> item);

        void ReturnToRoot(IDialogContext context, IAwaitable<IMessageActivity> result);
    }
}
