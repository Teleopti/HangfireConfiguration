using System;
using System.Collections.Generic;
using System.Linq;

namespace Hangfire.Configuration
{
    public class ConfigurationApi
    {
        private readonly IConfigurationStorage _storage;
        private readonly IHangfireSchemaCreator _creator;
        private readonly ConfigurationOptions _options;

        public ConfigurationApi(
            IConfigurationStorage storage,
            IHangfireSchemaCreator creator,
            ConfigurationOptions options)
        {
            _storage = storage;
            _creator = creator;
            _options = options;
        }

        public void WriteGoalWorkerCount(WriteGoalWorkerCount command)
        {
            if (command.Workers > _options.WorkerDeterminerOptions.MaximumGoalWorkerCount)
                throw new Exception("Invalid goal worker count.");

            var configurations = _storage.ReadConfigurations();
            var configuration = new StoredConfiguration();
            if (configurations.Any())
            {
                if (command.ConfigurationId != null)
                    configuration = configurations.FirstOrDefault(x => x.Id == command.ConfigurationId);
                else
                    configuration = configurations.FirstOrDefault(x => x.Active.GetValueOrDefault());

                if (configuration == null)
                    configuration = configurations.First();
            }

            configuration.GoalWorkerCount = command.Workers;

            _storage.WriteConfiguration(configuration);
        }

        public void CreateServerConfiguration(CreateServerConfiguration config)
        {
            var storageConnectionString = config.StorageConnectionString ??
                                          $"Data Source={config.Server};Initial Catalog={config.Database};User ID={config.User};Password={config.Password}";
            _creator.TryConnect(storageConnectionString);

            var creatorConnectionString = config.SchemaCreatorConnectionString ??
                                          $"Data Source={config.Server};Initial Catalog={config.Database};User ID={config.SchemaCreatorUser};Password={config.SchemaCreatorPassword}";
            _creator.TryConnect(creatorConnectionString);

            if (_creator.SchemaExists(config.SchemaName ?? DefaultSchemaName.Name(), creatorConnectionString))
                throw new Exception("Schema already exists.");

            _creator.CreateHangfireSchema(config.SchemaName, creatorConnectionString);

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

            if (!_options.AllowMultipleActive)
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