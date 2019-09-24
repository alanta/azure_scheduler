using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;

namespace AzureScheduler
{
    public static class TagHelper{
        public static bool AlwaysOn( this IResource resource ){
            return resource.HasTag("powerstate", "alwayson" );
        }

        public static bool HasTags(this IResource resource, IReadOnlyDictionary<string,string> tags )
        {
            return tags.All(t => resource.HasTag(t.Key, t.Value));
        }

        public static bool HasTag(this IResource resource, string name, string value)
        {
            return resource.Tags.HasTag(name, value);
        }

        public static bool HasTag(this IReadOnlyDictionary<string, string> tags, string name, string value)
        {
            return tags.TryGetValue(name, out var tagValue) && string.Equals(tagValue, value, StringComparison.OrdinalIgnoreCase);
        }
    }
}
