using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Hangfire.Configuration.Internals;
using Hangfire.Configuration.Providers;
using Npgsql;

namespace Hangfire.Configuration
{
	public class ConfigurationApi
	{
		private readonly IConfigurationStorage _storage;
		private readonly State _state;
		private readonly SqlDialectsServerConfigurationCreator _sqlDialectCreator;
		private readonly RedisServerConfigurationCreator _redisCreator;
		private readonly WorkerServerUpgrader _upgrader;

		internal ConfigurationApi(
			IConfigurationStorage storage,
			State state,
			SqlDialectsServerConfigurationCreator sqlDialectCreator,
			RedisServerConfigurationCreator redisCreator,
			WorkerServerUpgrader upgrader)
		{
			_storage = storage;
			_state = state;
			_sqlDialectCreator = sqlDialectCreator;
			_redisCreator = redisCreator;
			_upgrader = upgrader;
		}

		public void WriteGoalWorkerCount(WriteGoalWorkerCount command)
		{
			if (command.Workers > _state.ReadOptions().WorkerBalancerOptions.MaximumGoalWorkerCount)
				throw new Exception("Invalid goal worker count.");

			var configuration = matchingConfiguration(command.ConfigurationId);

			configuration.GoalWorkerCount = command.Workers;

			_storage.WriteConfiguration(configuration);
		}

		public void WriteMaxWorkersPerServer(WriteMaxWorkersPerServer command)
		{
			var configuration = matchingConfiguration(command.ConfigurationId);

			configuration.MaxWorkersPerServer = command.MaxWorkers;

			_storage.WriteConfiguration(configuration);
		}

		private StoredConfiguration matchingConfiguration(int? configurationId)
		{
			var configurations = _storage.ReadConfigurations();
			var configuration = new StoredConfiguration();
			if (configurations.Any())
			{
				if (configurationId != null)
					configuration = configurations.FirstOrDefault(x => x.Id == configurationId);
				else
					configuration = configurations.FirstOrDefault(x => x.IsActive());

				if (configuration == null)
					configuration = configurations.First();
			}

			return configuration;
		}

		public void CreateServerConfiguration(CreateSqlServerWorkerServer command)
		{
			var storage = new SqlConnectionStringBuilder
			{
				DataSource = command.Server ?? "",
				InitialCatalog = command.Database ?? "",
				UserID = command.User ?? "",
				Password = command.Password ?? "",
			}.ToString();
			var creator = new SqlConnectionStringBuilder
			{
				DataSource = command.Server ?? "",
				InitialCatalog = command.Database ?? "",
				UserID = command.SchemaCreatorUser ?? "",
				Password = command.SchemaCreatorPassword ?? "",
			}.ToString();

			_sqlDialectCreator.Create(
				storage,
				creator,
				command.SchemaName ?? storage.GetProvider().DefaultSchemaName(),
				command.Name
			);
		}

		public void CreateServerConfiguration(CreatePostgresWorkerServer command)
		{
			var storage = new NpgsqlConnectionStringBuilder
			{
				Host = command.Server,
				Database = command.Database,
				Username = command.User,
				Password = command.Password,
			}.ToString();
			var creator = new NpgsqlConnectionStringBuilder
			{
				Host = command.Server,
				Database = command.Database,
				Username = command.SchemaCreatorUser,
				Password = command.SchemaCreatorPassword,
			}.ToString();

			_sqlDialectCreator.Create(
				storage,
				creator,
				command.SchemaName ?? storage.GetProvider().DefaultSchemaName(),
				command.Name
			);
		}

		public void CreateServerConfiguration(CreateRedisWorkerServer command) =>
			_redisCreator.Create(command);

		public void ActivateServer(int configurationId)
		{
			var configurations = _storage.ReadConfigurations();

			var activate = configurations.Single(x => x.Id == configurationId);
			activate.Active = true;
			_storage.WriteConfiguration(activate);

			_state.PublisherQueryCache.Invalidate();
		}

		public void InactivateServer(int configurationId)
		{
			var configurations = _storage.ReadConfigurations();
			var inactivate = configurations.Single(x => x.Id == configurationId);
			inactivate.Active = false;
			if (!configurations.Any(x => x.Active==true))
				throw new ArgumentException("You must have at least one active configuration!");
			_storage.WriteConfiguration(inactivate);
		}

		public void UpgradeWorkerServers(UpgradeWorkerServers command) =>
			_upgrader.Upgrade(command);

		public IEnumerable<StoredConfiguration> ReadConfigurations() =>
			_storage.ReadConfigurations();

		public void WriteConfiguration(StoredConfiguration configuration) =>
			_storage.WriteConfiguration(configuration);
	}
}