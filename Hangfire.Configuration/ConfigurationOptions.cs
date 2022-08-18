using System.Collections.Generic;

namespace Hangfire.Configuration
{
    public class ConfigurationOptions
    {
        public string ConnectionString { get; set; }

        public bool PrepareSchemaIfNecessary { get; set; }
        
        public IEnumerable<UpdateStorageConfiguration> UpdateConfigurations { get; set; }

        public WorkerBalancerOptions WorkerBalancerOptions { get; } = new();

#if NETSTANDARD2_0
        public IHangfireConfigurationAuthorizationFilter Authorization { get; set; } = null;
#endif
	    
    }
}