using System.Collections.Generic;
using System.Linq;
using Hangfire.Configuration.Internals;
using Hangfire.Server;
#if NETSTANDARD2_0
using Microsoft.AspNetCore.Builder;
#else
using Owin;
#endif

namespace Hangfire.Configuration
{
	public class HangfireConfiguration
	{
		private readonly State _state;
		private object _builder;

		public HangfireConfiguration()
		{
			_state = new State(BuildNow());
		}
		
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

		public HangfireConfiguration UseStorageOptions(object storageOptions)
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

		public HangfireConfiguration StartWorkerServers()
		{
			BuildWorkerServerStarter().Start();
			return this;
		}

		public HangfireConfiguration StartWorkerServers(IEnumerable<IBackgroundProcess> additionalProcesses)
		{
			BuildWorkerServerStarter().Start(additionalProcesses.ToArray());
			return this;
		}

		public IEnumerable<ConfigurationInfo> QueryAllWorkerServers() =>
			BuildWorkerServerQueries().QueryAllWorkerServers();

		public IEnumerable<ConfigurationInfo> QueryPublishers() =>
			BuildPublisherQueries().QueryPublishers();

		public ConfigurationInfo GetPublisher(string connectionString, string schemaName) =>
			BuildPublisherQueries().GetPublisher(connectionString, schemaName);
		
		public ConfigurationApi ConfigurationApi() =>
			BuildConfigurationApi();

		internal ViewModelBuilder ViewModelBuilder() =>
			BuildViewModelBuilder();

		internal Options Options() => 
			BuildOptions();
		
		
		private StateMaintainer builderStateMaintainer(object appBuilder) =>
			new(BuildHangfire(appBuilder), BuildConfigurationStorage(),
				buildConfigurationUpdater(), _state);

		private ConfigurationUpdater buildConfigurationUpdater() => new(BuildConfigurationStorage(), _state);

		private Connector buildConnector() => new() {ConnectionString = _state.ReadOptions().ConnectionString};

		private WorkerBalancer buildWorkerDeterminer() => new(BuildKeyValueStore());

		protected ServerCountSampleRecorder buildServerCountSampleRecorder() =>
			new(
				BuildKeyValueStore(),
				_state,
				builderStateMaintainer(null),
				BuildNow());


		// outer services
		protected Options BuildOptions() => new(_state);

		protected WorkerServerStarter BuildWorkerServerStarter() =>
			new(BuildHangfire(_builder), buildWorkerDeterminer(),
				builderStateMaintainer(_builder), _state, buildServerCountSampleRecorder());

		protected PublisherStarter BuildPublisherStarter() => new(builderStateMaintainer(null), _state);

		protected ConfigurationApi BuildConfigurationApi() =>
			new(BuildConfigurationStorage(),
				_state,
				new SqlDialectsServerConfigurationCreator(BuildConfigurationStorage(), BuildSchemaInstaller()),
				new RedisServerConfigurationCreator(BuildConfigurationStorage(), BuildRedisConnectionVerifier()),
				new WorkerServerUpgrader(BuildSchemaInstaller(), BuildConfigurationStorage(), BuildOptions())
			);

		protected PublisherQueries BuildPublisherQueries() => new(_state, builderStateMaintainer(null));

		protected WorkerServerQueries BuildWorkerServerQueries() => new(builderStateMaintainer(null), _state);

		protected ViewModelBuilder BuildViewModelBuilder() => new(BuildConfigurationStorage());


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

		protected virtual IRedisConnectionVerifier BuildRedisConnectionVerifier() => new RedisConnectionVerifier();

	}
}