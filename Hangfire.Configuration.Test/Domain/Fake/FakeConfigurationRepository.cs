using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Hangfire.Configuration.Test.Domain.Fake;

public class FakeConfigurationStorage : IConfigurationStorage
{
	private readonly Func<ConfigurationOptions> _options;
	public IEnumerable<StoredConfiguration> Data => _data.Values;
	public int? Workers => Data.FirstOrDefault()?.GoalWorkerCount;
	public int? MaxWorkersPerServer => Data.FirstOrDefault()?.MaxWorkersPerServer;
	public int ReadConfigurationsQueryCount;
 
	private readonly ConcurrentDictionary<int, StoredConfiguration> _data = new();
	private int _nextId = 1;
	private int nextId() => _nextId++;

	public FakeConfigurationStorage(Func<ConfigurationOptions> options)
	{
		_options = options;
	}
	
	public IEnumerable<StoredConfiguration> ReadConfigurations()
	{
		// if (string.IsNullOrEmpty(_options.Invoke().ConnectionString))
		// 	throw new Exception("BOOM");
		ReadConfigurationsQueryCount++;
		return Data.Select(x => x.Copy()).ToArray();
	}

	public void WriteConfiguration(StoredConfiguration configuration)
	{
		configuration = configuration.Copy();
		configuration.Id ??= nextId();
		if (_data.ContainsKey(configuration.Id.Value))
			_data[configuration.Id.Value] = configuration;
		else
			_data.TryAdd(configuration.Id.Value, configuration);
	}

	public void Transaction(Action action) =>
		action.Invoke();

	public void LockConfiguration()
	{
	}

	public void HasGoalWorkerCount(int goalWorkerCount) => 
		Has(new StoredConfiguration {GoalWorkerCount = goalWorkerCount});

	public void Has(StoredConfiguration configuration) =>
		WriteConfiguration(configuration);

	public void Has(params StoredConfiguration[] configuration)
	{
		foreach (var storedConfiguration in configuration)
		{
			Has(storedConfiguration);
		}
	}

	public void Remove(int id) =>
		_data.TryRemove(id, out _);

	public void Clear() =>
		_data.Clear();
}