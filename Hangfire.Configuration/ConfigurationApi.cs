using System;
using System.Collections.Generic;
using System.Linq;

namespace Hangfire.Configuration
{
    public class ConfigurationApi
    {
        private readonly IConfigurationRepository _repository;
        private readonly IHangfireSchemaCreator _creator;
        private readonly ConfigurationOptions _options;

        public ConfigurationApi(
            IConfigurationRepository repository,
            IHangfireSchemaCreator creator,
            ConfigurationOptions options)
        {
            _repository = repository;
            _creator = creator;
            _options = options;
        }

        public void WriteGoalWorkerCount(WriteGoalWorkerCount command)
        {
            if (command.Workers > _options.MaximumGoalWorkerCount)
                throw new Exception("Invalid goal worker count.");

            var configurations = _repository.ReadConfigurations();
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

            _repository.WriteConfiguration(configuration);
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

            _repository.WriteConfiguration(new StoredConfiguration
            {
                ConnectionString = storageConnectionString,
                SchemaName = config.SchemaName,
                Active = false
            });
        }

        public void ActivateServer(int configurationId)
        {
            var configurations = _repository.ReadConfigurations();
            foreach (var configuration in configurations)
            {
                configuration.Active = configuration.Id == configurationId;
                _repository.WriteConfiguration(configuration);
            }
        }

        public IEnumerable<StoredConfiguration> ReadConfigurations() =>
            _repository.ReadConfigurations();

        public void WriteConfiguration(StoredConfiguration configuration) =>
            _repository.WriteConfiguration(configuration);
    }
}