using AuthBot;
using AzureBot.Domain;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureBot.Dialogs
{
    [Serializable]
    [LuisModel("77ae3bf7-695c-4cea-af77-31b1ba9d5940", "110c81d75bdb4f918a991696cd09f66b")]
    public class ResourceGroupDialog : AzureBotLuisDialog<string>
    {
        private static Lazy<string> resourceId = new Lazy<string>(() => ConfigurationManager.AppSettings["ActiveDirectory.ResourceId"]);

        [LuisIntent("CreateTemplateDeployment")]
        public async Task CreateResourceAsync(IDialogContext context)//, LuisResult result)
        {
            var accessToken = await context.GetAccessToken(resourceId.Value);
            if (string.IsNullOrEmpty(accessToken))
            {
                return;
            }

            var subscriptionId = context.GetSubscriptionId();

            //TODO - Take USer input for below parameters
            var groupName = "hackathonDemo";
            var storageName = "hacky2016";
            var deploymentName = "MyVmDeployment";
            var templateContainer = "templates";
            var templateJson = "azuredeploy.json";
            var parametersJSon = "azuredeploy.parameters.json";

            await new ResourceGroupDomain().CreateTemplateDeploymentAsync(accessToken, groupName, storageName, deploymentName, subscriptionId, templateContainer, templateJson, parametersJSon);

            await context.PostAsync($"Your VM deployment is initiated. Will let you know once deployment is completed. What's next?");

            context.Done(true);
            //(this.MessageReceived);

        }
    }
}
