using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using Microsoft.Extensions.Logging;

namespace AzureScheduler
{
    public static class ResourceHelper
    {
        public static async Task ProcessResources<TResource>( IEnumerable<TResource> resources, Func<TResource,bool> needsStateChange, Func<TResource,Task> action, ILogger log)
         where TResource:IResource
        {
            var azure = Configuration.GetAzureApi();
            var acis = 
                resources
                .Where(vm => vm.HasTags(Configuration.Tags()))
                .ToArray();

            log.LogInformation($"Found {acis.Count()} resources");

            var tasks = acis
                .Where(resource => needsStateChange(resource) && !resource.AlwaysOn())
                .Select(action);

            await Task.WhenAll(tasks);

            log.LogInformation("Done");
        }
    }
}
