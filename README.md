# HA Event Hubs Sample

This repository deploys a highly available Azure Event Hubs solution using the [Merge pattern](https://docs.microsoft.com/en-us/azure/event-hubs/event-hubs-federation-patterns#merge). The code in this repository is an adaptation of the [EventHubMerge](https://github.com/Azure-Samples/azure-messaging-replication-dotnet/tree/main/functions/code/EventHubMerge), such that an entire environment can be setup with a few clicks.

After deploying the ARM template in this repository, you will have the following in your Azure Subscription:

- Two newly created Resource Groups each representing an Event Hub region (defaults to East US and West US)
- Each Resource Group will contain:
    - Event Hub namespace
    - Event Hub
    - Function App to replicate local events to the remote Event Hub
    - Application Insights workspace for capturing Function App logs
    - Supporting Function App resources like App Service Plan and Storage Account

## Setup

Deploy the subscription ARM template in the `templates` folder directly from the GitHub repo by clicking the Deploy to Azure link below:

[![Deploy To Azure](https://raw.githubusercontent.com/Azure/azure-quickstart-templates/master/1-CONTRIBUTION-GUIDE/images/deploytoazure.svg?sanitize=true)](https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2Frjygraham%2FHAEventHub%2Fmain%2Ftemplates%2Fazuredeploy.json)

Or clone/download the repo and deploy from your local machine:

- Azure PowerShell (from root of repo): `New-AzSubscriptionDeployment -Location eastus -TemplateFile .\templates\azuredeploy.json`
- Azure CLI (from root of repo): `az deployment sub create --location eastus --template-file .\templates\azuredeploy.json`

## Usage

Complete the following steps once the ARM template has successfully deployed:

1. Find the Event Hub authorization rule connection string from one of the Event Hubs (hint: they are both an output of the ARM template).
1. Create and save a `Sensors.txt` file on your local computer with 3 lines - each line representing an ID of a sensor.
1. If you haven't already, download or clone this repository and open the `.\src\HAEventHubs.sln` Visual Studio solution.
1. Set `EventHubProducer` as the Startup Project and do one of the following:
    1. Replace the placeholder text in the `CommandLineOptions.cs` file in the `EventHubProducer` project with the Event Hub connection string and full path (including file name) to the `Sensors.txt` you created earlier.
    2. Specify the Connection String `-c` and Sensors File `-s` command line args during start-up of the `EventHubProducer`.
1. OPTIONAL: Edit and save `Sensors.txt` while the `EventHubProducer` to simulate the addition/removal of sensors.

## License

The MIT License (MIT)

Copyright © 2020 Ryan Graham

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the “Software”), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.