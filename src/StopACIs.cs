using System.Threading.Tasks;
using Microsoft.Azure.Management.ContainerInstance.Fluent;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace AzureScheduler
{
    public static class StopACIs
    {
        [FunctionName("StopACIs")]
        public static async Task Run([TimerTrigger("0 0 22 * * *")]TimerInfo myTimer, ILogger log)
        {
            var azure = Configuration.GetAzureApi();
            var acis = await azure.ContainerGroups.ListAsync();

            await ResourceHelper.ProcessResources(
                acis, 
                aci => aci.State == "Running", 
                aci => Stop(aci, log), log );
        }

        public static Task Stop(IContainerGroup containerGroup, ILogger log)
        {
            log.LogInformation("Stopping " + containerGroup.Name);
            return containerGroup.StopAsync();
        }
}
}
