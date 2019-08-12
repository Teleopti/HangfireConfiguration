using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace Hangfire.Configuration
{
    public class Configuration
    {
        private readonly IConfigurationRepository _repository;
        private readonly IHangfireSchemaCreator _creator;

        public Configuration(IConfigurationRepository repository, IHangfireSchemaCreator creator)
        {
            _repository = repository;
            _creator = creator;
        }

        public int? ReadGoalWorkerCount(int? configurationId = null)
        {
            return configurationId == null ? _repository.ReadConfigurations().FirstOrDefault()?.GoalWorkerCount : _repository.ReadConfigurations().FirstOrDefault(x => x.Id == configurationId)?.GoalWorkerCount;
        }

        public void WriteGoalWorkerCount(int? workers, int? configurationId = null)
        {
            var configurations = _repository.ReadConfigurations();
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

            configuration.GoalWorkerCount = workers;
            _repository.WriteConfiguration(configuration);
        }

        public IEnumerable<ServerConfigurationViewModel> BuildServerConfigurations()
        {
            var storedConfiguration = _repository.ReadConfigurations();

            return storedConfiguration.Select((x, i) => new ServerConfigurationViewModel
            {
                Id = x?.Id,
                ServerName = getServerName(x?.ConnectionString),
                DatabaseName = getDatabaseName(x?.ConnectionString),
                SchemaName = x?.SchemaName,
                Active = x?.Active == true ? "Active" : "Inactive",
                Workers = x?.GoalWorkerCount,
                Title = i == 0 ? "Default Hangfire configuration" : "Added Hangfire configuration"
            });
        }

        public void CreateServerConfiguration(CreateServerConfiguration config)
        {
            var storageConnectionString = $"Data Source={config.Server};Initial Catalog={config.Database};User ID={config.User};Password={config.Password}";
            _creator.TryConnect(storageConnectionString);

            var creatorConnectionString = $"Data Source={config.Server};Initial Catalog={config.Database};User ID={config.SchemaCreatorUser};Password={config.SchemaCreatorPassword}";
            _creator.TryConnect(creatorConnectionString);

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

        private void createEmptyDefault() =>
            _repository.WriteConfiguration(new StoredConfiguration());

        private string getDatabaseName(string connectionString)
        {
            var builder = new SqlConnectionStringBuilder(connectionString);
            return builder.InitialCatalog;
        }

        private string getServerName(string connectionString)
        {
            var builder = new SqlConnectionStringBuilder(connectionString);
            return builder.DataSource;
        }
    }
}