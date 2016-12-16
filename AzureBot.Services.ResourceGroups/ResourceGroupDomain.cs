using Microsoft.Azure;
using Microsoft.Azure.Management.Resources;
using Microsoft.Azure.Management.Resources.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureBot.Domain
{
    public class ResourceGroupDomain
    {
      public async Task<int> CreateTemplateDeploymentAsync(
      //  TokenCloudCredentials credential,
      string accessToken,
      string groupName,
      string storageName,
      string deploymentName,
      string subscriptionId,
      string containerName,
      string templateJson,
      string parametersJson)
        {
            try
            {
                var credentials = new TokenCloudCredentials(subscriptionId, accessToken);

                Console.WriteLine("Creating the template deployment...");
                var deployment = new Deployment();
                deployment.Properties = new DeploymentProperties
                {
                    Mode = DeploymentMode.Incremental,
                    TemplateLink = new TemplateLink
                    {
                        //Uri = new Uri("https://" + storageName + ".blob.core.windows.net/templates/azuredeploy.json")
                        Uri = new Uri("https://" + storageName + ".blob.core.windows.net/" + containerName + "/" + templateJson)
                    },
                    ParametersLink = new ParametersLink
                    {
                        //Uri = new Uri("https://" + storageName + ".blob.core.windows.net/templates/azuredeploy.parameters.json")
                        Uri = new Uri("https://" + storageName + ".blob.core.windows.net/" + containerName + "/" + parametersJson)
                    }
                };


                var resourceManagementClient = new ResourceManagementClient(credentials);

                await resourceManagementClient.Deployments.CreateOrUpdateAsync(
                  groupName,
                  deploymentName, deployment);
            }
            catch
            {

            }
            finally
            {

            }
            return 0;
        }


    }
}
