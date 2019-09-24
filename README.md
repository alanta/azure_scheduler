# Azure Scheduler

Automagically Start/Stop Azure Virtual Machines (VM) and Azure Container Instances (ACI) using Azure Functions with a timer trigger. 

This is intended to reduce costs for resources that are only used for development and testing.

One function starts all deallocated (stopped) VMs at 7:00 AM. The other function stops all VMs at 22:00.
Same thing is supported for ACIs. You can enable the functions you need.
VMs tagged with `powerstate=alwayson` are ignored. Additional filtering can be done by setting tags on resources.

## Requirements

You'll need Azure CLI and a development environment (VS Code or VS 2019) with Azure Functions support installed. If you want to try it out on your machine, please also install the Azure Storage Emulator.

## Local testing

You can run this Function App in the function app emulator using dev storage. Check the `local.settings.sample.json` file for details on how to set it up.

## Installation

In a nutshell, this is what you need to do to get this running for an Azure Subscription:

* Create a service account with appropriate rights in the Azure AD (AAD) that controls the subscription
* Deploy the Function App ( you can right-click deploy from VisualStudio, I won't tell a soul ;) ) or run it on a develepment machine from VS.
* Adjust the App Settings with the Client ID, Client Secret and AAD Tenant ID collected in the first step
* Ensure the Function App has the correct time zone. For example for West Europe, set `WEBSITE_TIME_ZONE` to `W. Europe Standard Time`. 

Note that:

* The function app does NOT need to run within the subscription where the VMs and ACIs are running.
* ACIs lose all state after stopping them, you'll have to setup persistance if you need it (for example an [Azure File Share](https://docs.microsoft.com/en-us/azure/container-instances/container-instances-volume-azure-files))
* This was tested with small numbers of ACIs and VMs (Up to 5)

## Assigning permissions with PowerShell

There's a poweshell script in the `deploy` folder: `create-service-principal.ps1`. It'll ask you to login and provide details. 
It used the VM manager role to enable the functions to start and stop VMs. There is no such role for ACIs, so the `ACI-manager-role.json` file contains an AAD role definition for such a role.

You'll need lots of rights in AAD to run the script and perform all the AAD actions. If you need to get an IT person involved in your org, be sure to send them _both_ files from the deploy folder.

## Manually assigning permissions to a service account

First login to azure (if you haven't already). Please ensure you have the right [subscription selected](https://docs.microsoft.com/en-us/cli/azure/account?view=azure-cli-latest#az-account-set).

```script
  az login
```

Then add an App to the Azure AD. Make sure to come up with a better password. This will be the _clientSecret_ for the Function app.

```script
  az ad app create --display-name vm-scheduler --password myReallyGoodPassword --identifier-uris vm-scheduler
```

Note the _applicationId_ in the output. It's a GUID and you'll need it in the next step and to configure the Azure function.
Next, create a service principal in the AAD.

```script
  az ad sp create --id <applicationId>
```

Note the _objectId_ in the output. It's a GUID again and you need it to assign the correct role to the service principal:

```script
  az role assignment create --assignee-object-id <objectId> --role "Virtual Machine Contributor"
```

Create a custom role for ACI management. Make sure you set the correct subscription id in that file

```script
 az role definition create --role-definition ACI-manager-role.json
```

Assign the custom role: 

```script
  az role assignment create --assignee-object-id <objectId> --role "Container Instance Manager"
```

Finally, get the AAD tenant id:

```script
  az account list --query "[].[name, tenantId]"
```

Now, deploy this Azure Functions App and configure the following app settings using the data collected above:

* ClientId
* ClientSecret
* TenantId

Finally, ensure the `WEBSITE_TIME_ZONE` is set to `W. Europe Standard Time`. 

## Controlling what resources to schedule

Any resource with the tag `powerstate=alwayson` will be skipped, both for starting and stopping.

You can control what resources to consider using the `Tags` app setting. For example:

```json
{ "team":"development" }
```

Will only consider resources tagged by the development team.

## Futher reading

* [Stop VMs with Azure Functions 1/3](https://medium.com/@rob_maas/stop-vms-with-azure-functions-101-1-3-187b80551daf)
* [Stop VMs with Azure Functions 2/3](https://medium.com/@rob_maas/stop-vms-with-azure-functions-2-3-5c7522c21ce2)
* [Create and remove Azure Container Instances from a TeamCity build](https://matthewdavis111.com/powershell/azure-container-instance-teamcity/)
* [The StackOverflow answer that prompted this repo being published on GitHub](https://stackoverflow.com/a/58065446/64096)

## License

This repo is available under the MIT license, so go ahead and fork / clone / do whatever you want. 

Please, don't call me when you accidentally shut down Prod resources...

## Contributing

Please send a pull request if you have improvements. Please keep PRs small and explain what you're fixing or adding. 
I'll do my best to follow up asap but I have other stuff to do as well...
