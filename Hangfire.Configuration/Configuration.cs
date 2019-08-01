using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace Hangfire.Configuration
{
    public class Configuration
    {
        private readonly IConfigurationRepository _repository;

        public Configuration(IConfigurationRepository repository) =>
            _repository = repository;

        public void WriteGoalWorkerCount(int? workers)
        {
            var configurations = _repository.ReadConfigurations();
            var configuration = new StoredConfiguration();
            if (configurations.Any())
            {
                configuration = configurations.FirstOrDefault(x => x.Active.GetValueOrDefault());
                if (configuration == null)
                    configuration = configurations.First();
            }

            configuration.GoalWorkerCount = workers;
            _repository.WriteConfiguration(configuration);
        }

        public int? ReadGoalWorkerCount() =>
            _repository.ReadConfigurations().FirstOrDefault()?.GoalWorkerCount;

        public IEnumerable<ServerConfigurationViewModel> BuildServerConfigurations()
        {
            var storedConfiguration = _repository.ReadConfigurations();

            return storedConfiguration.Select(x => new ServerConfigurationViewModel
            {
                Id = x?.Id,
                ServerName = getServerName(x?.ConnectionString),
                DatabaseName = getDatabaseName(x?.ConnectionString),
                SchemaName = x?.SchemaName,
                Active = x?.Active == true ? "Active" : "Inactive",
                Workers = x?.GoalWorkerCount
            });
        }

        public void CreateServerConfiguration(CreateServerConfiguration createServerConfiguration)
        {
            if (_repository.ReadConfigurations().IsEmpty())
                createEmptyDefault();
            
            var connectionStringBuilder = new SqlConnectionStringBuilder();
            connectionStringBuilder.DataSource = createServerConfiguration.Server;
            if (createServerConfiguration.Database != null)
                connectionStringBuilder.InitialCatalog = createServerConfiguration.Database;
            if (createServerConfiguration.User != null)
                connectionStringBuilder.UserID = createServerConfiguration.User;
            if (createServerConfiguration.Password != null)
                connectionStringBuilder.Password = createServerConfiguration.Password;

            var connectionString = connectionStringBuilder.ConnectionString;
            var schemaName = createServerConfiguration.SchemaName;
            
            _repository.WriteConfiguration(new StoredConfiguration()
            {
                ConnectionString = connectionString,
                SchemaName = schemaName,
                Active = false
            });
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

        public void ActivateServer(int configurationId)
        {
            var configurations = _repository.ReadConfigurations();
            foreach (var configuration in configurations)
            {
                configuration.Active = configuration.Id == configurationId;
                _repository.WriteConfiguration(configuration);
            }
        }
    }
}