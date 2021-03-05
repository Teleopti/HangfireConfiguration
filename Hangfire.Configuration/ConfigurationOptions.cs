using System.Collections.Generic;
#if NETSTANDARD2_0
using Microsoft.AspNetCore.Http;
#else
using Microsoft.Owin;
#endif

namespace Hangfire.Configuration
{
    public class ConfigurationOptions
    {
        public string ConnectionString { get; set; }

        public bool PrepareSchemaIfNecessary { get; set; }
        public bool AllowNewServerCreation { get; set; }
        public bool AllowMultipleActive { get; set; } = false;

        public string AutoUpdatedHangfireConnectionString { get; set; }
        public string AutoUpdatedHangfireSchemaName { get; set; }
        public IEnumerable<UpdateConfiguration> UpdateConfigurations { get; set; }

        public bool UseWorkerDeterminer { get; set; } = true;
        public WorkerDeterminerOptions WorkerDeterminerOptions { get; } = new WorkerDeterminerOptions();

        public IHangfireConfigurationAuthorizationFilter Authorization { get; set; } = null;
    }

    public interface IHangfireConfigurationAuthorizationFilter
    {
#if NETSTANDARD2_0
		bool Authorize(HttpContext context);
#else
	    bool Authorize(IOwinContext context);
#endif
	}

    public class WorkerDeterminerOptions
    {
        public int DefaultGoalWorkerCount { get; set; } = 10;
        public int MaximumGoalWorkerCount { get; set; } = 100;
        public int MinimumWorkerCount { get; set; } = 1;

        public int? MinimumServerCount { get; set; }
        public bool UseServerCountSampling { get; set; } = true;
    }

    public class UpdateConfiguration
    {
        public string Name { get; set; }
        public string ConnectionString { get; set; }
        public string SchemaName { get; set; }
    }
}