using System;
using System.Collections.Generic;
using System.Linq;
using Hangfire.Configuration.Internals;

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

        public void CreateServerConfiguration(CreateServerConfiguration config)
        {
			var storageConnectionString = config.StorageConnectionString ?? $"Data Source={config.Server};Initial Catalog={config.Database};User ID={config.User};Password={config.Password}";
			var creatorConnectionString = config.SchemaCreatorConnectionString ??
			                              $"Data Source={config.Server};Initial Catalog={config.Database};User ID={config.SchemaCreatorUser};Password={config.SchemaCreatorPassword}";
			
			if (!string.IsNullOrEmpty(config.DatabaseProvider) && config.DatabaseProvider == "PostgreSql")
			{
				storageConnectionString = config.StorageConnectionString ?? $@"Host={config.Server};Database=""{config.Database}"";User ID={config.User};Password={config.Password};";
				creatorConnectionString = config.SchemaCreatorConnectionString ??
				                              $@"Host={config.Server};Database=""{config.Database}"";User ID={config.SchemaCreatorUser};Password={config.SchemaCreatorPassword};";
			}

			_creator.TryConnect(storageConnectionString);
            
            _creator.TryConnect(creatorConnectionString);

            config.SchemaName ??= new ConnectionStringDialectSelector(creatorConnectionString)
	            .SelectDialect(DefaultSchemaName.SqlServer, DefaultSchemaName.Postgres);
            
            if (_creator.HangfireStorageSchemaExists(config.SchemaName, creatorConnectionString))
                throw new Exception("Schema already exists.");

            _creator.CreateHangfireStorageSchema(config.SchemaName, creatorConnectionString);

            _storage.WriteConfiguration(new StoredConfiguration
            {
                Name = config.Name,
                ConnectionString = storageConnectionString,
                SchemaName = config.SchemaName,
                Active = false
            });
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