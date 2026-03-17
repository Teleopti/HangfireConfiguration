using System;
using System.Collections.Generic;
using System.Linq;
using Hangfire.Configuration.Internals;
using Hangfire.Server;
#if NETSTANDARD2_0
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
#else
using Owin;
#endif

namespace Hangfire.Configuration;

public class HangfireConfiguration
{
	private readonly State _state;
	private object _builder;

	public HangfireConfiguration()
	{
		_state = new State(BuildNow());
	}

	public HangfireConfiguration UseApplicationBuilder(object builder)
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

	public IDisposable StartBackgroundJobServers() =>
		BuildBackgroundJobServerStarter().Start();

	public IDisposable StartBackgroundJobServers(IEnumerable<IBackgroundProcess> additionalProcesses) =>
		BuildBackgroundJobServerStarter().Start(additionalProcesses.ToArray());

	public IDisposable StartBackgroundProcesses(IEnumerable<IBackgroundProcess> processes) =>
		BuildBackgroundProcessStarter().Start(processes.ToArray());

	public IEnumerable<ConfigurationInfo> QueryAllBackgroundJobServers() =>
		BuildQueries().QueryAllBackgroundJobServers();

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

	private StateMaintainer builderStateMaintainer() => new(
		BuildHangfire(),
		BuildConfigurationStorage(),
		buildConfigurationUpdater(),
		_state,
		BuildNow());

	private ConfigurationUpdater buildConfigurationUpdater() => new(
		BuildConfigurationStorage(),
		_state,
		BuildNow());

	private Connector buildConnector() => new()
	{
		ConnectionString = _state.ReadOptions().ConnectionString
	};

	private WorkerBalancer buildWorkerBalancer() => new(BuildKeyValueStore());

	protected ServerCountSampleRecorder buildServerCountSampleRecorder() =>
		new(
			BuildKeyValueStore(),
			_state,
			builderStateMaintainer(),
			BuildNow());


	// outer services
	protected Options BuildOptions() => new(_state);

	protected BackgroundJobServerStarter BuildBackgroundJobServerStarter() =>
		new(BuildHangfire(),
			buildWorkerBalancer(),
			builderStateMaintainer(),
			_state,
			buildServerCountSampleRecorder(),
			_builder);

	protected BackgroundProcessStarter BuildBackgroundProcessStarter() =>
		new(BuildHangfire(),
			builderStateMaintainer(),
			_state,
			_builder);

	protected PublisherStarter BuildPublisherStarter() =>
		new(builderStateMaintainer(), _state);

	protected ConfigurationApi BuildConfigurationApi() =>
		new(BuildConfigurationStorage(),
			_state,
			new SqlDialectsServerConfigurationCreator(BuildConfigurationStorage(), BuildSchemaInstaller()),
			new RedisServerConfigurationCreator(BuildConfigurationStorage(), BuildRedisConnectionVerifier()),
			new StorageUpgrader(BuildSchemaInstaller(), BuildConfigurationStorage(), BuildOptions())
		);

	protected PublisherQueries BuildPublisherQueries() =>
		new(_state, builderStateMaintainer());

	protected Queries BuildQueries() =>
		new(builderStateMaintainer(), _state);

	protected ViewModelBuilder BuildViewModelBuilder() =>
		new(BuildConfigurationStorage());

	protected ConfigurationStorage BuildConfigurationStorage() =>
		new(BuildKeyValueStore());

	// boundary
	protected virtual IHangfire BuildHangfire() =>
		new RealHangfire();

	protected virtual ISchemaInstaller BuildSchemaInstaller() =>
		new SchemaInstaller();

	protected virtual IKeyValueStore BuildKeyValueStore() =>
		new KeyValueStore(buildConnector());

	protected virtual INow BuildNow() => new Now();

	protected virtual IRedisConnectionVerifier BuildRedisConnectionVerifier() =>
		new RedisConnectionVerifier();
}