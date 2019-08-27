using System.Collections.Generic;
using System.Linq;
using Hangfire.SqlServer;
using Newtonsoft.Json;

namespace Hangfire.Configuration
{
    public class HangfireStarter
    {
        private readonly IHangfire _hangfire;
        private readonly IConfigurationRepository _repository;

        public HangfireStarter(IHangfire hangfire, IConfigurationRepository repository)
        {
            _hangfire = hangfire;
            _repository = repository;
        }

        public IEnumerable<JobStorage> Start(ConfigurationOptions options, SqlServerStorageOptions storageOptions)
        {
            new DefaultServerConfigurator(_repository)
                .Configure(options?.DefaultHangfireConnectionString, options?.DefaultSchemaName);

            return _repository
                .ReadConfigurations()
                .OrderBy(x => !(x.Active ?? false))
                .ThenBy(x => x.Id)
                .Select(configuration => makeJobStorage(configuration, storageOptions))
                .ToArray();
        }
        
        private JobStorage makeJobStorage(StoredConfiguration configuration, SqlServerStorageOptions storageOptions)
        {
            var options = copyOptions(storageOptions ?? new SqlServerStorageOptions());
            options.SchemaName = configuration.SchemaName;
            var jobStorage = _hangfire.MakeSqlJobStorage(configuration.ConnectionString, options);
            
            if (configuration.Active == true)
                _hangfire.UseStorage(jobStorage);
            
            return jobStorage;
        }
        
        private static SqlServerStorageOptions copyOptions(SqlServerStorageOptions storageOptions) =>
            JsonConvert.DeserializeObject<SqlServerStorageOptions>(
                JsonConvert.SerializeObject(storageOptions)
            );
    }
}