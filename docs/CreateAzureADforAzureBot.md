# Setup Azure Active Directory for AzureBot Development

If you would like to authenticate against your own Azure AD for development or private use of the AzureBot code, you'll need to setup your own Azure AD application.  Here are the steps you need to follow:

1. Create or Utilize an Azure Active Directory (AAD) Tenant
2. Create a new Azure AD app on the AAD tenant
3. Configure the AAD application for use with the AzureBot
4. Configure App Multi-Tenancy (optional)

## 1. Create or Utilize an Azure Active Directory (AAD) tenant
 * You will need to create a new AAD application via the classic portal or via PowerShell.  To complete these steps, please [see this article](https://azure.microsoft.com/en-us/documentation/articles/active-directory-howto-tenant/).
 * Once it's created, get the Tenant ID by going to the [classic Azure portal](http://manage.windowsazure.com), select the Azure AD resource, and click on the view endpoints button.
 <br>![AzureAD-NewApplication](media/AzureAD-NewApp.jpg)
 * Note the tenant ID is the number after the 
 <br> ![AzureAD-GetTenantID.jpg](media/AzureAD-GetTenantID.jpg)

## 2. Create a new Azure AD app on the AAD tenant

  * In the same location as the previous step, click on add a new application. <br>
  
  * Select Add an application my organization is developing
  * Pick any name you want and select Web Application / Web API radio button
  * For both URLs pick the base domain name where you publish your Bot code to and then add /api/messages. <br>i.e. https://_mycustomsite_.azurewebsites.net/api/messages

> Note: If you plan to make your app multi-tenant enabled, you'll need to use a public domain name and get a corresponding SSL certificate.  See step 4 for more information.  You would then use something like https://_mycustomdomain.com_/api/messages instead.

## 3. Configure the AAD application for use with the AzureBot
Go to the configure tab of the application you just created and do the following:

  * Save the *Client ID* value. You will need this for the AzureBot development
  * Select duration for a key and click save.  Securely save the application secret key displayed on the screen - you will also need this for AzureBot Development.  You won't get access to this key again.
  * Add and save these two Reply URLs as follows: <br>https://_mycustomsite_.azurewebsites.com/api/OAuthCallback
  <br>https://localhost/api/OAuthCallback
  * Click on the green Add permissions to other applications button.  Select Windows Service Management API and check the checkbox.
  * Select Delegated Permissions and click the checkbox to "Access Azure Service Management..."
  <br> ![AzureAD-Permissions](media/AzureAD-Permissions.jpg)

## 4. Configure App Multitenancy
 
