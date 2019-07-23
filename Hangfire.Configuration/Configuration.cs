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

        public IEnumerable<ConfigurationViewModel> BuildConfigurations()
        {
            var storedConfiguration = _repository.ReadConfiguration();

            return storedConfiguration.Select(x => new ConfigurationViewModel()
            {
                Id = x?.Id,
                ServerName = getServerName(x?.ConnectionString),
                DatabaseName = getDatabaseName(x?.ConnectionString),
                SchemaName = x?.SchemaName,
                Active = x?.Active == true ? "Active" : "Inactive",
                Workers = x?.GoalWorkerCount
            });
        }

        public void SaveNewStorageConfiguration(NewStorageConfiguration newStorageConfiguration)
        {
            SqlConnectionStringBuilder connectionStringBuilder = new SqlConnectionStringBuilder();
            connectionStringBuilder["Data Source"] = newStorageConfiguration.Server;
            connectionStringBuilder["Initial Catalog"] = newStorageConfiguration.Database;
            connectionStringBuilder["User ID"] = newStorageConfiguration.User;
            connectionStringBuilder["Password"] = newStorageConfiguration.Password;

            var connectionString = connectionStringBuilder.ConnectionString;
            var schemaName = newStorageConfiguration.SchemaName;

            _repository.WriteNewStorageConfiguration(connectionString, schemaName, false);
        }

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

        public void ActivateStorage(int configurationId) =>
            _repository.ActivateStorage(configurationId);
    }

    public class NewStorageConfiguration
    {
        public string Server { get; set; }
        public string Database { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public string SchemaName { get; set; }
    }
}