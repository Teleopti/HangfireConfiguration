using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Hangfire.Configuration.Internals;
using Npgsql;

namespace Hangfire.Configuration
{
	public class ConfigurationApi
	{
		private readonly IConfigurationStorage _storage;
		private readonly IHangfireSchemaCreator _creator;
		private readonly State _state;

		internal ConfigurationApi(
			IConfigurationStorage storage,
			IHangfireSchemaCreator creator,
			State state)
		{
			_storage = storage;
			_creator = creator;
			_state = state;
		}

		public void WriteGoalWorkerCount(WriteGoalWorkerCount command)
		{
			if (command.Workers > _state.ReadOptions().WorkerDeterminerOptions.MaximumGoalWorkerCount)
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
					configuration = configurations.FirstOrDefault(x => x.Active.GetValueOrDefault());

				if (configuration == null)
					configuration = configurations.First();
			}

			return configuration;
		}

		public void CreateServerConfiguration(CreateSqlServerServerConfigurationCommand command)
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

			new SqlDialectsServerConfigurationCreator(_storage, _creator)
				.Create(
					storage,
					creator,
					command.SchemaName,
					command.Name
				);
		}

		public void CreateServerConfiguration(CreatePostgreSqlServerConfigurationCommand command)
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

			new SqlDialectsServerConfigurationCreator(_storage, _creator)
				.Create(
					storage,
					creator,
					command.SchemaName,
					command.Name
				);
		}

		public void CreateServerConfiguration(CreateRedisServerConfigurationCommand command)
		{
			new RedisServerConfigurationCreator(_storage)
				.Create(command);
		}

		public void ActivateServer(int configurationId)
		{
			var configurations = _storage.ReadConfigurations();

			var activate = configurations.Single(x => x.Id == configurationId);
			activate.Active = true;
			_storage.WriteConfiguration(activate);

			if (!_state.ReadOptions().AllowMultipleActive)
			{
				configurations
					.Where(x => x.Id != configurationId)
					.ForEach(inactivate =>
					{
						inactivate.Active = false;
						_storage.WriteConfiguration(inactivate);
					});
			}
		}

		public void InactivateServer(int configurationId)
		{
			var configurations = _storage.ReadConfigurations();
			var inactivate = configurations.Single(x => x.Id == configurationId);
			inactivate.Active = false;
			_storage.WriteConfiguration(inactivate);
		}

		public IEnumerable<StoredConfiguration> ReadConfigurations() =>
			_storage.ReadConfigurations();

		public void WriteConfiguration(StoredConfiguration configuration) =>
			_storage.WriteConfiguration(configuration);
	}
}