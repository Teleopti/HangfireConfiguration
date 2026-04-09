using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using Hangfire.Configuration.Test.Domain.Fake;

namespace Hangfire.Configuration.Test;

public class SystemUnderTest : HangfireConfiguration
{
	private FakeNow _now;

	public SystemUnderTest()
	{
		ApplicationBuilder = new ApplicationBuilder(null);
			
		UseApplicationBuilder(ApplicationBuilder);

		KeyValueStore = new FakeKeyValueStore();
		SchemaInstaller = new FakeSchemaInstaller();
		Monitor = new FakeMonitoringApi();
		Hangfire = new FakeHangfire(Monitor);
		RedisConnectionVerifier = new FakeRedisConnectionVerifier();

		UseOptions(new ConfigurationOptionsForTest());
		ConfigurationStorage = BuildConfigurationStorage();
		BackgroundJobServerStarter = new BackgroundJobServerStarterUnderTest(BuildBackgroundJobServerStarter(), BuildOptions());
		PublisherStarter = BuildPublisherStarter();
		Queries = BuildQueries();
		ViewModelBuilder = BuildViewModelBuilder();
		ServerCountSampleRecorder = buildServerCountSampleRecorder();
	}

	public ApplicationBuilder ApplicationBuilder { get; }

	public FakeMonitoringApi Monitor { get; }
	public FakeKeyValueStore KeyValueStore { get; }
	public FakeSchemaInstaller SchemaInstaller { get; }
	public FakeHangfire Hangfire { get; }
	public FakeRedisConnectionVerifier RedisConnectionVerifier { get; }

	public ConfigurationStorage ConfigurationStorage { get; }
	public BackgroundJobServerStarterUnderTest BackgroundJobServerStarter { get; }
	public PublisherStarter PublisherStarter { get; }
	public Queries Queries { get; }
	public ViewModelBuilder ViewModelBuilder { get; }
	public ServerCountSampleRecorder ServerCountSampleRecorder { get; }

	protected override IKeyValueStore BuildKeyValueStore() => KeyValueStore;
	protected override IHangfire BuildHangfire() => Hangfire;
	protected override ISchemaInstaller BuildSchemaInstaller() => SchemaInstaller;

	protected override INow BuildNow() =>
		_now ??= new FakeNow {Time = "2020-12-01 09:00".Utc()};

	protected override IRedisConnectionVerifier BuildRedisConnectionVerifier() => RedisConnectionVerifier;

	public SystemUnderTest WithConfiguration(StoredConfiguration configurations)
	{
		ConfigurationStorage.WriteConfiguration(configurations);
		return this;
	}
		
	public IEnumerable<StoredConfiguration> Configurations() => 
		ConfigurationStorage.ReadConfigurations();

	public SystemUnderTest HasGoalWorkerCount(int goalWorkerCount)
	{
		ConfigurationStorage.WriteConfiguration(new StoredConfiguration
		{
			Containers = new[] { new ContainerConfiguration { GoalWorkerCount = goalWorkerCount } }
		});
		return this;
	}

	public void ClearConfigurations()
	{
		ConfigurationStorage.ReadConfigurations()
			.ForEach(x => ConfigurationStorage.DeleteConfiguration(x));
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

	public SystemUnderTest WithServerCountSample(ServerCountSample sample)
	{
		KeyValueStore.Has(sample);
		return this;
	}

	public SystemUnderTest WithGoalWorkerCount(int goalWorkerCount)
	{
		var configuration = getConfiguration();
		configuration.Containers ??= new[] { new ContainerConfiguration() };
		configuration.Containers[0].GoalWorkerCount = goalWorkerCount;
		ConfigurationStorage.WriteConfiguration(configuration);
		return this;
	}

	public SystemUnderTest WithMaxWorkersPerServer(int maxWorkersPerServer)
	{
		var configuration = getConfiguration();
		configuration.Containers ??= new[] { new ContainerConfiguration() };
		configuration.Containers[0].MaxWorkersPerServer = maxWorkersPerServer;
		ConfigurationStorage.WriteConfiguration(configuration);
		return this;
	}

	private StoredConfiguration getConfiguration()
	{
		var existing = ConfigurationStorage.ReadConfigurations();
		if (!existing.Any())
			ConfigurationStorage.WriteConfiguration(new StoredConfiguration());
		return ConfigurationStorage.ReadConfigurations().First();
	}
}