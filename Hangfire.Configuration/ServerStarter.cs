using System.Collections.Generic;
using System.Linq;
using Hangfire.Server;
using Hangfire.SqlServer;
using Newtonsoft.Json;
using Owin;

namespace Hangfire.Configuration
{
    public interface IHangfire
    {
        IAppBuilder UseHangfireServer(
            IAppBuilder builder,
            JobStorage storage,
            BackgroundJobServerOptions options,
            params IBackgroundProcess[] additionalProcesses);

        JobStorage MakeSqlJobStorage(string connectionString, SqlServerStorageOptions options);
    }

    public class RealHangfire : IHangfire
    {
        public IAppBuilder UseHangfireServer(
            IAppBuilder builder,
            JobStorage storage,
            BackgroundJobServerOptions options,
            params IBackgroundProcess[] additionalProcesses) =>
            builder.UseHangfireServer(storage, options, additionalProcesses);

        public JobStorage MakeSqlJobStorage(string connectionString, SqlServerStorageOptions options) =>
            new SqlServerStorage(connectionString, options);
    }

    public class ServerStarter
    {
        private readonly IAppBuilder _builder;
        private readonly IHangfire _hangfire;
        private readonly IConfigurationRepository _repository;

        public ServerStarter(IAppBuilder builder, IHangfire hangfire, IConfigurationRepository repository)
        {
            _builder = builder;
            _hangfire = hangfire;
            _repository = repository;
        }

        public IEnumerable<RunningServer> StartServers(
            ConfigurationOptions options,
            BackgroundJobServerOptions serverOptions,
            SqlServerStorageOptions storageOptions,
            params IBackgroundProcess[] additionalProcesses)
        {
            var serverNumber = 1;
            new DefaultServerConfigurator(_repository).Configure(options?.DefaultHangfireConnectionString, options?.DefaultSchemaName);

            var storedConfigs = _repository.ReadConfigurations().ToArray();
            var activeConfiguration = storedConfigs.FirstOrDefault(x => x.Active ?? false) ?? storedConfigs.FirstOrDefault();

            return storedConfigs.Select(storedConfig =>
                {
                    var appliedStorageOptions = buildSqlServerStorageOptions(storageOptions, storedConfig.SchemaName);
                    var sqlJobStorage = _hangfire.MakeSqlJobStorage(storedConfig.ConnectionString, appliedStorageOptions);

                    var workerCount = WorkerDeterminer.DetermineWorkerCount
                    (
                        sqlJobStorage.GetMonitoringApi(),
                        storedConfig.GoalWorkerCount,
                        options ?? new ConfigurationOptions()
                    );
                    var appliedServerOptions = buildBackgroundJobServerOptions(serverOptions, workerCount);

                    var appliedAdditionalProcess = new IBackgroundProcess[] { };
                    if (storedConfig.Id == activeConfiguration.Id)
                        appliedAdditionalProcess = additionalProcesses;

                    _hangfire.UseHangfireServer(_builder, sqlJobStorage, appliedServerOptions, appliedAdditionalProcess);

                    var runningServer = new RunningServer() {Number = serverNumber, Storage = sqlJobStorage};
                    serverNumber++;

                    return runningServer;
                })
                .ToArray();
        }

        private BackgroundJobServerOptions buildBackgroundJobServerOptions(BackgroundJobServerOptions serverOptions, int workerCount)
        {
            var appliedServerOptions = new BackgroundJobServerOptions();
            
            if (serverOptions != null)
            {
                appliedServerOptions.Queues = serverOptions.Queues;
                appliedServerOptions.HeartbeatInterval = serverOptions.HeartbeatInterval;
                appliedServerOptions.ServerTimeout = serverOptions.ServerTimeout;
                appliedServerOptions.ShutdownTimeout = serverOptions.ShutdownTimeout;
                appliedServerOptions.StopTimeout = serverOptions.StopTimeout;
                appliedServerOptions.CancellationCheckInterval = serverOptions.CancellationCheckInterval;
                appliedServerOptions.SchedulePollingInterval = serverOptions.SchedulePollingInterval;
                appliedServerOptions.ServerCheckInterval = serverOptions.ServerCheckInterval;
            }

            appliedServerOptions.WorkerCount = workerCount;
            
            return appliedServerOptions;
        }

        private SqlServerStorageOptions buildSqlServerStorageOptions(SqlServerStorageOptions storageOptions, string schemaName)
        {
            var appliedStorageOptions = new SqlServerStorageOptions();
            if (storageOptions != null)
            {
                var options = JsonConvert.SerializeObject(storageOptions);
                appliedStorageOptions = JsonConvert.DeserializeObject<SqlServerStorageOptions>(options);
            }

            appliedStorageOptions.SchemaName = schemaName;

            return appliedStorageOptions;
        }
    }
}