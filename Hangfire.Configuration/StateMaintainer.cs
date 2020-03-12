using System.Linq;
using Hangfire.SqlServer;
using Newtonsoft.Json;

namespace Hangfire.Configuration
{
    public class StateMaintainer
    {
        private readonly IHangfire _hangfire;
        private readonly IConfigurationRepository _repository;
        private readonly ConfigurationUpdater _configurationUpdater;
        private readonly State _state;
        private readonly object _lock = new object();

        internal StateMaintainer(IHangfire hangfire, IConfigurationRepository repository, ConfigurationUpdater configurationUpdater, State state)
        {
            _hangfire = hangfire;
            _repository = repository;
            _configurationUpdater = configurationUpdater;
            _state = state;
        }

        public void Refresh(ConfigurationOptions options, SqlServerStorageOptions storageOptions)
        {
            // maybe not reload all the time
            var configurations = _repository.ReadConfigurations();
            var configurationChanged = _configurationUpdater.Update(options, configurations);
            if (configurationChanged)
                configurations = _repository.ReadConfigurations();

            lock (_lock)
            {
                _state.Configurations = configurations
                    .Select(c =>
                    {
                        var existing = _state.Configurations.SingleOrDefault(x => x.Configuration.Id == c.Id);
                        if (existing != null)
                        {
                            existing.Configuration = c;
                            return existing;
                        }

                        return makeJobStorage(c, storageOptions);
                    }).ToArray();
            }
        }

        private ConfigurationAndStorage makeJobStorage(StoredConfiguration configuration, SqlServerStorageOptions storageOptions)
        {
            var options = copyOptions(storageOptions ?? new SqlServerStorageOptions());
            if (string.IsNullOrEmpty(configuration.SchemaName))
                options.SchemaName = DefaultSchemaName.Name();
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