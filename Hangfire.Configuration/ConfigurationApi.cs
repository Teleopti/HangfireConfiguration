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
		private readonly ISchemaInstaller _installer;
		private readonly ITryConnectToRedis _tryConnectToRedis;
		private readonly State _state;

		internal ConfigurationApi(
			IConfigurationStorage storage,
			ISchemaInstaller installer,
			ITryConnectToRedis tryConnectToRedis,
			State state)
		{
			_storage = storage;
			_installer = installer;
			_tryConnectToRedis = tryConnectToRedis;
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

			new SqlDialectsServerConfigurationCreator(_storage, _installer)
				.Create(
					storage,
					creator,
					command.SchemaName ?? DefaultSchemaName.SqlServer(),
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

			new SqlDialectsServerConfigurationCreator(_storage, _installer)
				.Create(
					storage,
					creator,
					command.SchemaName ?? DefaultSchemaName.Postgres(),
					command.Name
				);
		}

		public void CreateServerConfiguration(CreateRedisWorkerServer command)
		{
			new RedisServerConfigurationCreator(_storage, _tryConnectToRedis)
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

		public void UpgradeWorkerServers(UpgradeWorkerServers command)
		{
			_installer.InstallHangfireConfigurationSchema(_state.ReadOptions().ConnectionString);
			var configurations = _storage.ReadConfigurations();
			configurations
				.Where(x => !string.IsNullOrEmpty(x.ConnectionString))
				.Where(x => x.ConnectionString.ToDbVendorSelector().SelectDialect(true, true, false))
				.ForEach(x =>
				{
					var schemaName = x.SchemaName ?? x.ConnectionString.ToDbVendorSelector()
						.SelectDialect(DefaultSchemaName.SqlServer(), DefaultSchemaName.Postgres());

					var connectionString = x.ConnectionString;
					if (command.SchemaUpgraderUser != null)
						connectionString = connectionString.SetUserNameAndPassword(command.SchemaUpgraderUser, command.SchemaUpgraderPassword);

					_installer.InstallHangfireStorageSchema(schemaName, connectionString);
				});
		}

		public IEnumerable<StoredConfiguration> ReadConfigurations() =>
			_storage.ReadConfigurations();

		public void WriteConfiguration(StoredConfiguration configuration) =>
			_storage.WriteConfiguration(configuration);
	}
}