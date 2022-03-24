#if !NET472
using Microsoft.AspNetCore.Builder;
#else
using Microsoft.Owin.Builder;
#endif
using System;
using System.Linq;
using Hangfire.Configuration.Test.Domain.Fake;
using Hangfire.SqlServer;

namespace Hangfire.Configuration.Test
{
    public class SystemUnderTest : HangfireConfiguration
    {
        public SystemUnderTest()
        {
#if !NET472
            ApplicationBuilder = new ApplicationBuilder(null);
#else
            ApplicationBuilder = new AppBuilder();
#endif
	        UseApplicationBuilder(ApplicationBuilder);
	        
            ConfigurationStorage = new FakeConfigurationStorage();
            KeyValueStore = new FakeKeyValueStore();
            SchemaInstaller = new FakeSchemaInstaller();
            Monitor = new FakeMonitoringApi();
            Hangfire = new FakeHangfire(ApplicationBuilder, Monitor);
            _now = new FakeNow {Time = "2020-12-01 09:00".Utc()};
            RedisConfigurationVerifier = new FakeRedisConfigurationVerifier();
            
            Options = BuildOptions();
            Options.UseOptions(new ConfigurationOptions
            {
	            ConnectionString = "unknown-storage"
            });
			ConfigurationApi = BuildConfigurationApi();
            WorkerServerStarter = new WorkerServerStarterUnderTest(BuildWorkerServerStarter(), Options);
            PublisherStarter = new PublisherStarterUnderTest(BuildPublisherStarter(), Options);
            PublisherQueries = new PublisherQueriesUnderTest(BuildPublisherQueries(), Options);
            WorkerServerQueries = new WorkerServerQueriesUnderTest(BuildWorkerServerQueries(), Options);
            ViewModelBuilder = BuildViewModelBuilder();
            ServerCountSampleRecorder = buildServerCountSampleRecorder();
        }

#if !NET472
        public ApplicationBuilder ApplicationBuilder { get; }
#else
        public AppBuilder ApplicationBuilder { get; }
#endif

        public FakeMonitoringApi Monitor { get; }
        public FakeConfigurationStorage ConfigurationStorage { get; }
        public FakeKeyValueStore KeyValueStore { get; }
        public FakeSchemaInstaller SchemaInstaller { get; }
        public FakeHangfire Hangfire { get; }
        public FakeRedisConfigurationVerifier RedisConfigurationVerifier;
        private FakeNow _now;

        public Options Options { get; }
        public ConfigurationApi ConfigurationApi { get; }
        public WorkerServerStarterUnderTest WorkerServerStarter { get; }
        public PublisherStarterUnderTest PublisherStarter { get; }
        public PublisherQueriesUnderTest PublisherQueries { get; }
        public WorkerServerQueriesUnderTest WorkerServerQueries { get; }
        public ViewModelBuilder ViewModelBuilder { get; }
        public ServerCountSampleRecorder ServerCountSampleRecorder { get; }

        protected override IConfigurationStorage BuildConfigurationStorage() => ConfigurationStorage;
        protected override IKeyValueStore BuildKeyValueStore() => KeyValueStore;
        protected override IHangfire BuildHangfire(object appBuilder) => Hangfire;
        protected override ISchemaInstaller BuildSchemaInstaller() => SchemaInstaller;
        protected override INow BuildNow() => _now;
        protected override IRedisConfigurationVerifier BuildRedisConfigurationVerifier() => RedisConfigurationVerifier;

        public SystemUnderTest WithConfiguration(StoredConfiguration configurations)
        {
            ConfigurationStorage.Has(configurations);
            return this;
        }

        public SystemUnderTest WithAnnouncedServer(string serverId)
        {
            Monitor.AnnounceServer(serverId);
            return this;
        }

        public class FakeNow : INow
        {
            public DateTime Time;
            public DateTime UtcDateTime() => Time;
        }

        public SystemUnderTest Now(string time) => Now(time.Utc());

        public SystemUnderTest Now(DateTime time)
        {
            _now.Time = time;
            return this;
        }

        public void StartWorkerServer()
        {
	        WorkerServerStarter.Start(null, null, (SqlServerStorageOptions)null);
        }

        public SystemUnderTest WithServerCountSample(ServerCountSample sample)
        {
            KeyValueStore.Has(sample);
            return this;
        }

        public SystemUnderTest WithOptions(ConfigurationOptions options)
        {
	        Options.UseOptions(options);
	        return this;
        }

        public SystemUnderTest WithGoalWorkerCount(int goal)
        {
	        configuration.GoalWorkerCount = goal;
	        return this;
        }
        
        public SystemUnderTest WithMaxWorkersPerServer(int maxWorkers)
        {
	        configuration.MaxWorkersPerServer = maxWorkers;
	        return this;
        }

        private StoredConfiguration configuration
        {
	        get
	        {
		        if (!ConfigurationStorage.Data.Any())
			        ConfigurationStorage.Has(new StoredConfiguration());

		        return ConfigurationStorage.Data.First();
		        
	        }
        }
    }
}