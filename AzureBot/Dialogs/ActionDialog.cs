namespace AzureBot.Dialogs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.Luis;
    using Microsoft.Bot.Builder.Luis.Models;
    using Microsoft.Bot.Connector;
    using Microsoft.Bot.Builder.FormFlow;
    using FormTemplates;
    using Azure.Management.ResourceManagement;
    [LuisModel("c9e598cb-0e5f-48f6-b14a-ebbb390a6fb3", "a7c1c493d0e244e796b83c6785c4be4d")]
    [Serializable]
    public class ActionDialog : LuisDialog<string>
    {
        private static string[] ordinals = { "first", "second", "third", "fourth", "fifth" };

        private readonly string originalMessage;

        private readonly ILuisService service;

        public ActionDialog(string originalMessage)
        {
            this.originalMessage = originalMessage;
            if (service == null)
            {
                var type = this.GetType();
                var luisModel = type.GetCustomAttribute<LuisModelAttribute>(inherit: true);
                if (luisModel == null)
                {
                    throw new Exception("Luis model attribute is not set for the class");
                }

                service = new LuisService(luisModel);
            }
        }


        public override async Task StartAsync(IDialogContext context)
        {
            var luisResult = await this.service.QueryAsync(this.originalMessage);


            await ListVmsAsync(context, luisResult);
        }

        protected override Task MessageReceived(IDialogContext context, IAwaitable<Message> item)
        {
            return base.MessageReceived(context, item);
        }

        [LuisIntent("")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            string message = $"Sorry I did not understand: " + string.Join(", ", result.Intents.Select(i => i.Intent));
            await context.PostAsync(message);
            context.Wait(MessageReceived);
        }

        [LuisIntent("ListSubscriptions")]
        public async Task ListSubscriptionsAsync(IDialogContext context, LuisResult result)
        {
            int index = 0;
            var subscriptions = GetAllSubscriptions().Aggregate(string.Empty, (current, next) =>
            {
                index++;
                return current += $"\r\n{index}. {next}";
            });

            await context.PostAsync($"Your subscriptions are: {subscriptions}");
            context.Wait(MessageReceived);
        }

        [LuisIntent("UseSubscription")]
        public async Task UseSubscriptionAsync(IDialogContext context, LuisResult result)
        {
            var entity = result.Entities.OrderByDescending(p => p.Score).FirstOrDefault();
            if (entity != null)
            {
                var subscriptionName = entity.Entity;
                if (entity.Type == "builtin.ordinal")
                {
                    var ordinal = Array.IndexOf(ordinals, entity.Entity.ToLowerInvariant());
                    if (ordinal >= 0)
                    {
                        subscriptionName = GetAllSubscriptions().ElementAt(ordinal);
                    }
                }
                await context.PostAsync($"Using the {subscriptionName} subscription.");
            }
            else
            {
                await context.PostAsync("Which subscription do you want to use?");
            }

            context.Wait(MessageReceived);
        }

        [LuisIntent("ListVms")]
        public async Task ListVmsAsync(IDialogContext context, LuisResult result)
        {
            int index = 0;
            var virtualMachines = GetAllVms().Aggregate(string.Empty, (current, next) =>
            {
                index++;
                return current += $"\r\n{index}. {next}";
            });

            await context.PostAsync($"Available VMs are: {virtualMachines}");
            context.Wait(MessageReceived);
        }

        [LuisIntent("StartVm")]
        public async Task StartVmAsync(IDialogContext context, LuisResult result)
        {
            var subscriptionId = context.PerUserInConversationData.Get<string>("SubscriptionId");
            var form = new FormDialog<VirtualMachineFormState>(
                new VirtualMachineFormState(), 
                Forms.BuildVirtualMachinesForm, 
                FormOptions.PromptInStart, 
                result.Entities);
            context.Call(form, this.VirtualMachineFormComplete);
        }

        private async Task VirtualMachineFormComplete(IDialogContext context, IAwaitable<VirtualMachineFormState> result)
        {
            try
            {
                var virtualMachine = await result;
                var subscriptionId = context.PerUserInConversationData.Get<string>("SubscriptionId");
                await context.PostAsync($"Starting the {virtualMachine.Name} virtual machine.");
                await (new AzureRepository().StartVirtualMachineAsync(subscriptionId, virtualMachine.Name));
            }
            catch (FormCanceledException<VirtualMachineFormState> ex)
            {
                await context.PostAsync("You have canceled the operation. What would you like to do next?");
            }

            context.Wait(this.MessageReceived);
        }

        [LuisIntent("StopVm")]
        public async Task StopVmAsync(IDialogContext context, LuisResult result)
        {
            var entity = result.Entities.OrderByDescending(p => p.Score).FirstOrDefault();
            if (entity != null)
            {
                var virtualMachineName = entity.Entity;
                if (entity.Type == "builtin.ordinal")
                {
                    var ordinal = Array.IndexOf(ordinals, entity.Entity.ToLowerInvariant());
                    if (ordinal >= 0)
                    {
                        virtualMachineName = GetAllVms().ElementAt(ordinal);
                    }
                }

                await context.PostAsync($"Stopping the {virtualMachineName} virtual machine.");
            }
            else
            {
                await context.PostAsync("Which virtual machine do you want to stop?");
            }

            context.Wait(MessageReceived);
        }

        [LuisIntent("RunRunbook")]
        public async Task RunRunbookAsync(IDialogContext context, LuisResult result)
        {
            var entity = result.Entities.OrderByDescending(p => p.Score).FirstOrDefault();
            if (entity != null)
            {
                var runbookName = entity.Entity;
                await context.PostAsync($"Launching the {runbookName} runbook.");
            }
            else
            {
                await context.PostAsync("Which runbook do you want to run?");
            }

            context.Wait(MessageReceived);
        }

        // TODO - move Azure operations to a separate class
        private IEnumerable<string> GetAllSubscriptions()
        {
            return new string[] { "Development", "Staging", "Production", "QA" };
        }

        private IEnumerable<string> GetAllVms()
        {
            return new string[] { "svrcomm01", "svrcomm02", "svrapipub", "svrdbprod" };
        }
    }
}