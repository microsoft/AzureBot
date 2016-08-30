# Creating Azurebot Conversations

## Overview
AzureBot uses LUIS as part of the Bot Framework to map natural language, conversational messages from a user to a specific _intent_ and _entity_ which is understood by AzureBot. When AzureBot identifies an intent and entity, it triggers a function or set of functions.

1. User sends message to AzureBot
> List VMs
2. AzureBot uses LUIS model to identify the intent, entity, and attribute of the message sent by the user
> intent: "list", entity:"vm"
3. AzureBot calls the function
```
[LuisIntent("ListVms")]
public async Task ListVmsAsync(IDialogContext context, LuisResult result)
```
4. AzureBot responds to the user
<blockquote>Available VMs are:
<ul>
	<li>fsjwinvm (Status: Running, ResourceGroup: FSJWINVM, Size: Basic_A1)</li>
	<li>fsjtest (Status: Running, ResourceGroup: FSJTEST, Size: Basic_A1)</li>
</ul>

</blockquote>
<!--TODO: Add image-->
![conversation overview](http://placehold.it/650x150 "Overview")

## AzureBot Intent Structure

AzureBot Intents are separated into CRUD type operations

Intent      | AzureBot Actions 
--- | ---
Create | <ul><li>Create Resource</li><li>Clone Resource</li></ul>
Read | <ul><li>List Resources</li><li>Find/Search</li><li>Show</li><li>Help</li></ul> 
Update | <ul><li>Start</li><li>Stop</li><li>Shutdown</li><li>Set</li><li>Manage</li><li>Monitor</li></ul>
Delete | <ul><li>Delete</li></ul>
Authenticate | <ul><li>Login</li><li>Logout</li></ul>

## AzureBot Entities
AzureBot entities map to the [Microsoft Azure Resource Manager](https://azure.microsoft.com/en-us/documentation/articles/resource-group-overview/) (ARM) Resources we manipulate

* Virtual Machines
* Subscriptions
* Azure Automation Runbooks

## Examples
Message: "start vm mytestvm"
Intent: _Update_
Entity: _VirtualMachine_
Attribute/Context: _mytestvm_


## References
* [LUIS Understanding Natural Language](https://docs.botframework.com/en-us/node/builder/guides/understanding-natural-language/)
* [Azure Resource Manager Overview](https://azure.microsoft.com/en-us/documentation/articles/resource-group-overview/)