using Hangfire.Storage;
using Owin;

namespace Hangfire.Configuration
{
    public class CompositionRoot
    {
        public ServerStarter BuildServerStarter(IAppBuilder appBuilder, ConfigurationOptions options) 
        {
            return new ServerStarter(appBuilder, BuildHangfire(), BuildRepository(options.ConnectionString));
        }

        public virtual IHangfire BuildHangfire()
        {
            return new RealHangfire();
        }

        public WorkerDeterminer BuildWorkerDeterminer(string connectionString)
        {
            return new WorkerDeterminer(BuildConfiguration(connectionString), BuildMonitoringApi());
        }

        public virtual IMonitoringApi BuildMonitoringApi()
        {
            return JobStorage.Current.GetMonitoringApi();
        }

        public Configuration BuildConfiguration(string connectionString)
        {
            return new Configuration(BuildRepository(connectionString), BuildHangfireSchemaCreator());
        }

        public virtual IHangfireSchemaCreator BuildHangfireSchemaCreator()
        {
            return new HangfireSchemaCreator();
        }

        public virtual IConfigurationRepository BuildRepository(string connectionString)
        {
            return new ConfigurationRepository(connectionString);
        }
    }
}