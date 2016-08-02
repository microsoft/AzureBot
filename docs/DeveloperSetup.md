# Developer Setup

## Overview

If you would like to contribute to the Azure Bot or run it on your own environment, these instructions will help you get set up. 

The solution was created using [Visual Studio 2015 Update 2](https://www.visualstudio.com/en-us/news/vs2015-update2-vs.aspx). It was built with the [Microsoft Bot framework] (http://docs.botframework.com/) and the [Microsoft Bot Builder C# SDK](http://docs.botframework.com/sdkreference/csharp/). It uses the [Azure Resource Manager Nuget package](https://www.nuget.org/packages/Microsoft.Azure.Management.ResourceManager) and other Azure packages.

## Table of Contents
- [LUIS Model](#luis)
- [Azure Active Directory](#ad)
- [Bot Framework Emulator](#bot)
- [Putting it all Together](#doit)

## LUIS Model

AzureBot uses [Language Understanding Intelligent Service (LUIS)](https://www.luis.ai), a part of [Microsoft Cognitive Services](https://www.microsoft.com/cognitive-services/), to understand the user's intent. 

The AzureBot code in this repo has the the ***LuisModel*** attribute on the ***ActionDialog*** class already configured with our LUIS model.

If you would like to develop with your own model and extend it with your additions to the bot, [create an account with LUIS](https://www.luis.ai). Then create a new application using New App -> Import Existing App. Then, in this new app dialog box, upload the [AzureBot.json file](../AzureBot/LuisModel/AzureBot.json).   

Once you have created the new LUIS application, train and [publish it](https://www.luis.ai/Help/#PublishingModel), then update the ***LuisModel*** attribute in your ***ActionDialog*** class with the new application ID and subscription key. You can get the application ID and subscription key from the LUIS application published URL.

## Azure Active Directory

If you would like to authenticate against your own Azure AD for development or private use of the AzureBot code, you'll need to setup your own Azure AD application. Follow our [Azure AD instructions](/CreateAzureADforAzureBot.md) to get it set up.

## Bot Framework Emulator

If you would like to run the bot locally from Visual Studio and test it, the Bot Framework Emulator is very helpful.

Follow the [instructions to install the Bot Framework Emulator](https://docs.botframework.com/en-us/tools/bot-framework-emulator/) on your development environment. 

## Putting it all Together

***Instructions to be added***