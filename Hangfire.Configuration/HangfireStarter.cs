using System.Collections.Generic;
using System.Linq;
using Hangfire.SqlServer;
using Newtonsoft.Json;

namespace Hangfire.Configuration
{
    public class HangfireStarter
    {
        private readonly IHangfireStorage _hangfireStorage;
        private readonly IConfigurationRepository _repository;
        private readonly IDistributedLock _distributedLock;

        public HangfireStarter(IHangfireStorage hangfireStorage, IConfigurationRepository repository, IDistributedLock distributedLock)
        {
            _hangfireStorage = hangfireStorage;
            _repository = repository;
            _distributedLock = distributedLock;
        }

        public IEnumerable<StorageWithConfiguration> Start(ConfigurationOptions options, SqlServerStorageOptions storageOptions)
        {
            new DefaultServerConfigurator(_repository, _distributedLock)
                .Configure(options?.DefaultHangfireConnectionString, options?.DefaultSchemaName);

            return _repository
                .ReadConfigurations()
                .OrderBy(x => !(x.Active ?? false))
                .ThenBy(x => x.Id)
                .Select(configuration => makeJobStorage(configuration, storageOptions))
                .ToArray();
        }
        
        private StorageWithConfiguration makeJobStorage(StoredConfiguration configuration, SqlServerStorageOptions storageOptions)
        {
            var options = copyOptions(storageOptions ?? new SqlServerStorageOptions());
            options.SchemaName = configuration.SchemaName;
            var jobStorage = _hangfireStorage.MakeSqlJobStorage(configuration.ConnectionString, options);
            
            if (configuration.Active == true)
                _hangfireStorage.UseStorage(jobStorage);

            return new StorageWithConfiguration
            {
                Configuration = configuration,
                JobStorage = jobStorage
            };
        }
        
        private static SqlServerStorageOptions copyOptions(SqlServerStorageOptions storageOptions) =>
            JsonConvert.DeserializeObject<SqlServerStorageOptions>(
                JsonConvert.SerializeObject(storageOptions)
            );
    }

    public class StorageWithConfiguration
    {
        public StoredConfiguration Configuration;
        public JobStorage JobStorage;
    }
}