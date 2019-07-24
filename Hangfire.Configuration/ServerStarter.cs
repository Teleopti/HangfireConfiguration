using System.Linq;
using Hangfire.Server;
using Hangfire.SqlServer;
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
        private readonly Configuration _configuration;
        private readonly IHangfire _hangfire;

        public ServerStarter(IAppBuilder builder, Configuration configuration, IHangfire hangfire)
        {
            _builder = builder;
            _configuration = configuration;
            _hangfire = hangfire;
        }

        public void StartServers(BackgroundJobServerOptions serverOptions, SqlServerStorageOptions storageOptions, params IBackgroundProcess[] additionalProcesses)
        {
//            _configuration.BuildConfigurations().Select((item, index) =>
//                index == 0 ? _useHangfireServer.UseHangfireServer(_builder, new FakeJobStorage(), serverOptions, additionalProcesses) : 
//                             _useHangfireServer.UseHangfireServer(_builder, new FakeJobStorage(), serverOptions)).ToArray();

            var configs = _configuration.ReadConfigurations().ToArray();
            var firstConfig = configs.FirstOrDefault();
            
            var sqlStorageOptions = configs.Select(c => new SqlServerStorageOptions()
            {
                SchemaName = c.SchemaName,
                PrepareSchemaIfNecessary = storageOptions?.PrepareSchemaIfNecessary ?? false
            }).ToArray();
                
            foreach (var config in configs)
            {
                if (config == firstConfig)
                    _hangfire.UseHangfireServer(_builder, _hangfire.MakeSqlJobStorage(config.ConnectionString, sqlStorageOptions.FirstOrDefault()), serverOptions, additionalProcesses);
                else
                    _hangfire.UseHangfireServer(_builder, _hangfire.MakeSqlJobStorage(config.ConnectionString, sqlStorageOptions.LastOrDefault()), serverOptions);
            }
        }
        
    }
}