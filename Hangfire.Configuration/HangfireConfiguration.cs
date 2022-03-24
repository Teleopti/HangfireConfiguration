using System;
using System.Collections.Generic;
using System.Linq;
using Hangfire.Configuration.Internals;
using Hangfire.PostgreSql;
using Hangfire.Server;
using Hangfire.SqlServer;
#if NETSTANDARD2_0
using Microsoft.AspNetCore.Builder;

#else
using Owin;

#endif

namespace Hangfire.Configuration
{
    public class HangfireConfiguration
    {
#if NETSTANDARD2_0
	    public HangfireConfiguration UseApplicationBuilder(IApplicationBuilder builder)
#else
        public HangfireConfiguration UseApplicationBuilder(IAppBuilder builder)
#endif
	    {
		    _builder = builder;
		    return this;
	    }
	    
	    public HangfireConfiguration UseOptions(ConfigurationOptions options)
	    {
		    BuildOptions().UseOptions(options);
		    return this;
	    }
	    
        public HangfireConfiguration UseStorageOptions(SqlServerStorageOptions storageOptions)
        {
            BuildOptions().UseStorageOptions(storageOptions);
            return this;
        }

        public HangfireConfiguration UseStorageOptions(PostgreSqlStorageOptions storageOptions)
        {
	        BuildOptions().UseStorageOptions(storageOptions);
	        return this;
        }

		public HangfireConfiguration UseServerOptions(BackgroundJobServerOptions serverOptions)
        {
            BuildOptions().UseServerOptions(serverOptions);
            return this;
        }

        public HangfireConfiguration StartPublishers()
        {
            BuildPublisherStarter().Start();
            return this;
        }

        public HangfireConfiguration StartWorkerServers(IEnumerable<IBackgroundProcess> additionalProcesses)
        {
            BuildWorkerServerStarter()
                .Start(additionalProcesses.ToArray());
            return this;
        }

        public IEnumerable<ConfigurationInfo> QueryAllWorkerServers()
        {
            return BuildWorkerServersQuerier()
                .QueryAllWorkerServers();
        }

        public IEnumerable<ConfigurationInfo> QueryPublishers()
        {
            return BuildPublishersQuerier()
                .QueryPublishers();
        }

        public ConfigurationApi ConfigurationApi() =>
            BuildConfigurationApi();

        internal ViewModelBuilder ViewModelBuilder() =>
            BuildViewModelBuilder();
        
        
        
        
        // internal services
		private readonly State _state = new();

		private StateMaintainer builderStateMaintainer(object appBuilder) =>
			new(BuildHangfire(appBuilder), BuildConfigurationStorage(),
				buildConfigurationUpdater(), _state);

		private ConfigurationUpdater buildConfigurationUpdater() => new(BuildConfigurationStorage(), _state);

		private Connector buildConnector() => new() {ConnectionString = _state.ReadOptions().ConnectionString};

		private WorkerDeterminer buildWorkerDeterminer() => new(BuildKeyValueStore());

		protected ServerCountSampleRecorder buildServerCountSampleRecorder() =>
			new(
				BuildKeyValueStore(),
				_state,
				builderStateMaintainer(null),
				BuildNow());


		// outer services
		public Options BuildOptions() => new(_state);
		private object _builder;

		public WorkerServerStarter BuildWorkerServerStarter() =>
			new(BuildHangfire(_builder), buildWorkerDeterminer(),
				builderStateMaintainer(_builder), _state, buildServerCountSampleRecorder());

		public PublisherStarter BuildPublisherStarter() => new(builderStateMaintainer(null), _state);

		public ConfigurationApi BuildConfigurationApi() =>
			new(BuildConfigurationStorage(),
				_state,
				new SqlDialectsServerConfigurationCreator(BuildConfigurationStorage(), BuildSchemaInstaller()),
				new RedisServerConfigurationCreator(BuildConfigurationStorage(), BuildRedisConfigurationVerifier()),
				new WorkerServerUpgrader(BuildSchemaInstaller(), BuildConfigurationStorage(), BuildOptions())
			);

		public PublisherQueries BuildPublishersQuerier() => new(_state, builderStateMaintainer(null));

		public WorkerServerQueries BuildWorkerServersQuerier() => new(builderStateMaintainer(null), _state);

		public ViewModelBuilder BuildViewModelBuilder() => new(BuildConfigurationStorage());

		// boundary
		protected virtual IHangfire BuildHangfire(object appBuilder) =>
			new RealHangfire(appBuilder);

		protected virtual ISchemaInstaller BuildSchemaInstaller() =>
			new SchemaInstaller();

		protected virtual IConfigurationStorage BuildConfigurationStorage() =>
			new ConfigurationStorage(buildConnector());

		protected virtual IKeyValueStore BuildKeyValueStore() =>
			new KeyValueStore(buildConnector());

		protected virtual INow BuildNow() => new Now();
		
		protected virtual IRedisConfigurationVerifier BuildRedisConfigurationVerifier() => new RedisConfigurationVerifier();

    }
}