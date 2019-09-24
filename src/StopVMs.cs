using System.Threading.Tasks;
using Microsoft.Azure.Management.Compute.Fluent;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace AzureScheduler
{
    public static class StopVMs
    {
        [FunctionName("StopVMs")]
        public static async Task Run([TimerTrigger("0 0 22 * * *")]TimerInfo myTimer, ILogger log)
        {
            var azure = Configuration.GetAzureApi();
            var vms = await azure.VirtualMachines.ListAsync();

            await ResourceHelper.ProcessResources(vms,
                vm => vm.PowerState == PowerState.Running,
                vm => Stop(vm, log),
                log);
        }

        public static Task Stop(IVirtualMachine vm, ILogger log)
        {
            log.LogInformation("Stopping {name} (current state: {state})", vm.Name, vm.PowerState.ToString());
            return vm.DeallocateAsync();
        }
    }
}

