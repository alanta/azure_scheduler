using System;
using System.Collections.Generic;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Newtonsoft.Json;

namespace AzureScheduler
{
    public static class Configuration{
            public static Func<string> ClientId = () => Environment.GetEnvironmentVariable("ClientId");
            public static Func<string> ClientSecret = () => Environment.GetEnvironmentVariable("ClientSecret");
            public static Func<string> TenantId = () => Environment.GetEnvironmentVariable("TenantId");
            public static Func<IReadOnlyDictionary<string,string>> Tags = () => ParseTags(Environment.GetEnvironmentVariable("Tags"));

            private static IReadOnlyDictionary<string, string> ParseTags(string value)
            {
                if( string.IsNullOrWhiteSpace(value))
                    return new Dictionary<string, string>();

                return JsonConvert.DeserializeObject<Dictionary<string, string>>(value);
            }

            public static IAzure GetAzureApi(){
                var credentials = SdkContext.AzureCredentialsFactory.FromServicePrincipal(Configuration.ClientId(), Configuration.ClientSecret(), Configuration.TenantId(), AzureEnvironment.AzureGlobalCloud);            
                return Azure.Authenticate(credentials).WithDefaultSubscription();
            }
    }
}
