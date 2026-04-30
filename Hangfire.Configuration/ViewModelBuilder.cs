using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Hangfire.Configuration.Internals;
using Npgsql;

namespace Hangfire.Configuration;

public class ViewModelBuilder
{
	private readonly ConfigurationStorage _storage;
	private readonly Options _options;
	private readonly QueueCalculator _queueCalculator;
	private readonly StateMaintainer _stateMaintainer;

	internal ViewModelBuilder(
		ConfigurationStorage storage,
		Options options,
		QueueCalculator queueCalculator,
		StateMaintainer stateMaintainer)
	{
		_storage = storage;
		_options = options;
		_queueCalculator = queueCalculator;
		_stateMaintainer = stateMaintainer;
	}

	public IEnumerable<ViewModel> BuildServerConfigurations()
	{
		_stateMaintainer.Refresh();
		var availableQueues = _options.ServerOptions()?.Queues ?? [];
		return _storage.ReadConfigurations()
			.Select(x =>
			{
				var schemaName = x.AppliedSchemaName();
				var connectionString = hidePassword(x);

				return new ViewModel
				{
					Id = x.Id,
					Name = x.Name,
					ConnectionString = connectionString,
					SchemaName = schemaName,
					Active = x.IsActive(),
					Containers = x.Containers.Select(c => new ContainerViewModel
					{
						Tag = c.Tag,
						AppliedQueues = _queueCalculator.CalculateAppliedQueues(c, x.Containers),
						SelectedQueues = c.Queues ?? [],
						WorkerBalancerEnabled = c.WorkerBalancerIsEnabled(),
						Workers = c.GoalWorkerCount,
						MaxWorkersPerServer = c.MaxWorkersPerServer
					}).ToArray(),
					AvailableQueues = availableQueues
				};
			}).ToArray();
	}

	private static string hidePassword(StoredConfiguration x)
	{
		const string hiddenPassword = "******";
		return x.ConnectionString.ToDbSelector().PickFunc(
			() =>
			{
				var parsed = new SqlConnectionStringBuilder(x.ConnectionString);
				if (string.IsNullOrEmpty(parsed.Password))
					return x.ConnectionString;
				parsed.Password = hiddenPassword;
				return parsed.ToString();
			},
			() =>
			{
				var parsed = new NpgsqlConnectionStringBuilder(x.ConnectionString);
				if (string.IsNullOrEmpty(parsed.Password))
					return x.ConnectionString;
				parsed.Password = hiddenPassword;
				return parsed.ToString();
			},
			() =>
			{
#if Redis
				var parsed = StackExchange.Redis.ConfigurationOptions.Parse(x.ConnectionString);
				if (parsed.Password != null)
					parsed.Password = hiddenPassword;
				return parsed.ToString();
#else
				return x.ConnectionString;
#endif
			});
	}
}