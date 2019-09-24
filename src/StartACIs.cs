using System.Threading.Tasks;
using Microsoft.Azure.Management.ContainerInstance.Fluent;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace AzureScheduler
{
    public static class StartACIs
    {
        [FunctionName("StartACIs")]
        public static async Task Run([TimerTrigger("0 0 7 * * 1-5")]TimerInfo myTimer, ILogger log)
        {
            var azure = Configuration.GetAzureApi();
            var acis = await azure.ContainerGroups.ListAsync();

            await ResourceHelper.ProcessResources(
                acis,
                aci => aci.State != "Running",
                aci => Start(aci, log),
                log);
        }

        public static Task Start(IContainerGroup containerGroup, ILogger log)
        {
            log.LogInformation("Starting " + containerGroup.Name);
            return ContainerGroupsOperationsExtensions.StartAsync(containerGroup.Manager.Inner.ContainerGroups, containerGroup.ResourceGroupName, containerGroup.Name);
        }
    }
}