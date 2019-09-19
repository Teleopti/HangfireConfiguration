using System.Linq;
using Hangfire.SqlServer;
using Newtonsoft.Json;

namespace Hangfire.Configuration
{
    public class StorageCreator
    {
        private readonly IHangfire _hangfire;
        private readonly IConfigurationRepository _repository;
        private readonly ConfigurationAutoUpdater _configurationAutoUpdater;
        private readonly State _state;

        internal StorageCreator(IHangfire hangfire, IConfigurationRepository repository, ConfigurationAutoUpdater configurationAutoUpdater, State state)
        {
            _hangfire = hangfire;
            _repository = repository;
            _configurationAutoUpdater = configurationAutoUpdater;
            _state = state;
        }

        public void Refresh(ConfigurationOptions options, SqlServerStorageOptions storageOptions)
        {
            // shouldnt do this all the time
            _configurationAutoUpdater.Update(options);
            
            // maybe not reload all the time
            var configurations = _repository.ReadConfigurations();
            configurations.ForEach(c =>
            {
                var existing = _state.Configurations.SingleOrDefault(x => x.Configuration.Id == c.Id);
                if (existing != null)
                    existing.Configuration = c;
                else
                    _state.Configurations = _state.Configurations.Append(makeJobStorage(c, storageOptions)).ToArray();
            });
        }

        private ConfigurationAndStorage makeJobStorage(StoredConfiguration configuration, SqlServerStorageOptions storageOptions)
        {
            var options = copyOptions(storageOptions ?? new SqlServerStorageOptions());
            if (string.IsNullOrEmpty(configuration.SchemaName))
                options.SchemaName = "HangFire";
            else
                options.SchemaName = configuration.SchemaName;

            return new ConfigurationAndStorage
            {
                JobStorageCreator = () => _hangfire.MakeSqlJobStorage(configuration.ConnectionString, options),
                Configuration = configuration
            };
        }

        private static SqlServerStorageOptions copyOptions(SqlServerStorageOptions storageOptions) =>
            JsonConvert.DeserializeObject<SqlServerStorageOptions>(
                JsonConvert.SerializeObject(storageOptions)
            );
    }
}