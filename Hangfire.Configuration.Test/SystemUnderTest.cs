using Microsoft.AspNetCore.Builder;
using System;
using System.Linq;
using Hangfire.Configuration.Test.Domain.Fake;

namespace Hangfire.Configuration.Test
{
	public class SystemUnderTest : HangfireConfiguration
	{
		private FakeNow _now;

		public SystemUnderTest()
		{
			ApplicationBuilder = new ApplicationBuilder(null);
			
			UseApplicationBuilder(ApplicationBuilder);

			ConfigurationStorage = new FakeConfigurationStorage();
			KeyValueStore = new FakeKeyValueStore();
			SchemaInstaller = new FakeSchemaInstaller();
			Monitor = new FakeMonitoringApi();
			Hangfire = new FakeHangfire(ApplicationBuilder, Monitor);
			RedisConfigurationVerifier = new FakeRedisConfigurationVerifier();

			UseOptions(new ConfigurationOptions
			{
				ConnectionString = "unknown-storage"
			});
			WorkerServerStarter = new WorkerServerStarterUnderTest(BuildWorkerServerStarter(), BuildOptions());
			PublisherStarter = BuildPublisherStarter();
			PublisherQueries = new PublisherQueriesUnderTest(BuildPublisherQueries(), BuildOptions());
			WorkerServerQueries = BuildWorkerServerQueries();
			ViewModelBuilder = BuildViewModelBuilder();
			ServerCountSampleRecorder = buildServerCountSampleRecorder();
		}

		public ApplicationBuilder ApplicationBuilder { get; }

		public FakeMonitoringApi Monitor { get; }
		public FakeConfigurationStorage ConfigurationStorage { get; }
		public FakeKeyValueStore KeyValueStore { get; }
		public FakeSchemaInstaller SchemaInstaller { get; }
		public FakeHangfire Hangfire { get; }
		public FakeRedisConfigurationVerifier RedisConfigurationVerifier { get; }

		public WorkerServerStarterUnderTest WorkerServerStarter { get; }
		public PublisherStarter PublisherStarter { get; }
		public PublisherQueriesUnderTest PublisherQueries { get; }
		public WorkerServerQueries WorkerServerQueries { get; }
		public ViewModelBuilder ViewModelBuilder { get; }
		public ServerCountSampleRecorder ServerCountSampleRecorder { get; }

		protected override IConfigurationStorage BuildConfigurationStorage() => ConfigurationStorage;
		protected override IKeyValueStore BuildKeyValueStore() => KeyValueStore;
		protected override IHangfire BuildHangfire(object appBuilder) => Hangfire;
		protected override ISchemaInstaller BuildSchemaInstaller() => SchemaInstaller;

		protected override INow BuildNow() =>
			_now ??= new FakeNow {Time = "2020-12-01 09:00".Utc()};

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

		public SystemUnderTest Now(string time) => Now(time.Utc());

		public SystemUnderTest Now(DateTime time)
		{
			_now.Time = time;
			return this;
		}

		public void StartWorkerServer()
		{
			WorkerServerStarter.Start();
		}

		public SystemUnderTest WithServerCountSample(ServerCountSample sample)
		{
			KeyValueStore.Has(sample);
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