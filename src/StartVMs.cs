using System.Threading.Tasks;
using Microsoft.Azure.Management.Compute.Fluent;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace AzureScheduler
{
    public static class StartVMs
    {
        [FunctionName("StartVMs")]
        public static async Task Run([TimerTrigger("0 0 7 * * 1-5")]TimerInfo myTimer, ILogger log)
        {
            var azure = Configuration.GetAzureApi();
            var vms = await azure.VirtualMachines.ListAsync();

            await ResourceHelper.ProcessResources(vms,
                vm => vm.PowerState == PowerState.Deallocated,
                vm => Start(vm, log),
                log);
        }

        public static Task Start(IVirtualMachine vm, ILogger log)
        {
            log.LogInformation("Starting {name} (current state: {state})", vm.Name, vm.PowerState.ToString() );
            return vm.StartAsync();
        }
    }
}