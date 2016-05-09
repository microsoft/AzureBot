using System;
using System.Threading.Tasks;
using AzureBot.Models;
using Microsoft.Bot.Builder.Dialogs;

namespace AzureBot.Dialogs
{
    public class VirtualMachineDialog : IDialog<VirtualMachine>
    {
        public Task StartAsync(IDialogContext context)
        {
            throw new NotImplementedException();
        }
    }
}