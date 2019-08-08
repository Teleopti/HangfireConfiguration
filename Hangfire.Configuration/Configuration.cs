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

        public int? ReadGoalWorkerCount(int? configurationId = null)
        {
            return configurationId == null ? 
                _repository.ReadConfigurations().FirstOrDefault()?.GoalWorkerCount : 
                _repository.ReadConfigurations().FirstOrDefault(x => x.Id == configurationId )?.GoalWorkerCount;
        }
        
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
                Title = i == 0 ? "Default Hangfire Server" : "Added Hangfire Server" 
            });
        }
        
        public void CreateServerConfiguration(CreateServerConfiguration createServerConfiguration)
        {
            if (_repository.ReadConfigurations().IsEmpty())
                createEmptyDefault();

            var connectionString = createConnectionString(createServerConfiguration);
            var connectionStringForCreate = createConnectionString(createServerConfiguration, true);
            
            _repository.CreateHangfireSchema(createServerConfiguration.SchemaName, connectionStringForCreate);

            _repository.WriteConfiguration(new StoredConfiguration()
            {
                ConnectionString = connectionString,
                SchemaName = createServerConfiguration.SchemaName,
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

        private string createConnectionString(CreateServerConfiguration createServerConfiguration, bool forCreate = false)
        {
            var connectionStringBuilder = new SqlConnectionStringBuilder()
            {
                DataSource = createServerConfiguration.Server,
                InitialCatalog = createServerConfiguration.Database,
                UserID = forCreate ? createServerConfiguration.UserForCreate : createServerConfiguration.User,
                Password = forCreate ? createServerConfiguration.PasswordForCreate: createServerConfiguration.Password
            };

            _repository.TryConnect(connectionStringBuilder.ConnectionString);

            return connectionStringBuilder.ConnectionString;
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