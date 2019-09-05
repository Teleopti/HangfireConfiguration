using System.Collections.Generic;
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
        private readonly HangfireStorageState _storageState;

        internal StorageCreator(IHangfire hangfire, IConfigurationRepository repository, ConfigurationAutoUpdater configurationAutoUpdater, HangfireStorageState storageState)
        {
            _hangfire = hangfire;
            _repository = repository;
            _configurationAutoUpdater = configurationAutoUpdater;
            _storageState = storageState;
        }

        public IEnumerable<HangfireStorage> Create(ConfigurationOptions options, SqlServerStorageOptions storageOptions)
        {
            _configurationAutoUpdater.Update(options);
            create(storageOptions, _repository.ReadConfigurations());
            return _storageState.StorageState;
        }

        public void CreateActive(ConfigurationOptions options, SqlServerStorageOptions storageOptions)
        {
            _configurationAutoUpdater.Update(options);
            create(storageOptions, _repository.ReadConfigurations().Where(x => x.Active.GetValueOrDefault()));
        }

        private void create(
            SqlServerStorageOptions storageOptions, 
            IEnumerable<StoredConfiguration> configurations)
        {
            if (_storageState.StorageState != null)
                return;
            _storageState.StorageState = configurations
                .OrderBy(x => !(x.Active ?? false))
                .ThenBy(x => x.Id)
                .Select(configuration => makeJobStorage(configuration, storageOptions))
                .ToArray();
        }

        private HangfireStorage makeJobStorage(StoredConfiguration configuration, SqlServerStorageOptions storageOptions)
        {
            var options = copyOptions(storageOptions ?? new SqlServerStorageOptions());
            options.SchemaName = configuration.SchemaName;

            var storage = new HangfireStorage
            {
                Configuration = configuration,
                JobStorage = _hangfire.MakeSqlJobStorage(configuration.ConnectionString, options)
            };

            return storage;
        }

        private static SqlServerStorageOptions copyOptions(SqlServerStorageOptions storageOptions) =>
            JsonConvert.DeserializeObject<SqlServerStorageOptions>(
                JsonConvert.SerializeObject(storageOptions)
            );
    }
}