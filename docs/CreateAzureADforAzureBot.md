# Setup Azure Active Directory for AzureBot Development

If you would like to authenticate against your own Azure AD for development or private use of the AzureBot code, you'll need to setup your own Azure AD application.  Here are the steps you need to follow:

1. Create or Utilize an Azure Active Directory (AAD) tenant<br>
You will need to create a new AAD application via the classic portal or via PowerShell.  To complete these steps, please [see this article](https://azure.microsoft.com/en-us/documentation/articles/active-directory-howto-tenant/). 

2. Create a new Azure AD app on the AAD tenant
* Go to the [classic Azure portal](http://manage.windowsazure.com) and add a new application. <br>
![AzureAD-NewApplication](.\media\AzureAD-NewApp.jpg)
* Select Add an application my organization is developing
* Pick any name you want and select Web Application / Web API radio button
* For both URLs pick the base domain name you'd like to publish your Bot code to  and then add /api/messages. <br>i.e. https://mycustomsite.azurewebsites.net/api/messages

3. Configure the AAD application for use with the AzureBot
<br> Go to the configure tab of the application you just created and do the following:
* Save the *Client ID* value. You will need this for the AzureBot development
* Select duration for a key and click save.  Securely save the application secret key displayed on the screen - you will also need this for AzureBot Development.  You won't get access to this key again.
* Add and save these two Reply URLs as follows: <br>https://mycustomsite.azurewebsites.com/api/OAuthCallback
<br>https://localhost/api/OAuthCallback
* Click on the green Add permissions to other applications button.  Select Windows Service Management API and check the checkbox.
* Select Delegated Permissions and click the checkbox to "Access Azure Service Management..."
<br> ![AzureAD-Permissions](.\media\AzureAD-Permissions.jpg)