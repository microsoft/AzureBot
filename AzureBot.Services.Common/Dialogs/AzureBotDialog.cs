using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;

namespace AzureBot.Dialogs
{
    [Serializable]
    public abstract class AzureBotDialog<T> : IAzureBotDialog<T>
    {
        public virtual async Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceived);
        }

        public virtual async Task MessageReceived(IDialogContext context, IAwaitable<IMessageActivity> item)
        {
            var message = await item;
            var messageText = message.Text;
            context.Wait(MessageReceived);
        }

        public virtual async Task UnableToHandleUserMessage(IDialogContext context, IAwaitable<IMessageActivity> item)
        {
            var message = await item;
            ReturnToRoot(context, item);
        }

        public virtual void ReturnToRoot(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            context.Done(result);
        }
    }
}