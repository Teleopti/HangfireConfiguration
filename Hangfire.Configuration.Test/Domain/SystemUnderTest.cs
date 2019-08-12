using Hangfire.Configuration.Test.Domain.Fake;
using Microsoft.Owin.Builder;

namespace Hangfire.Configuration.Test.Domain
{
    public class SystemUnderTest
    {
        public SystemUnderTest()
        {
            AppBuilder = new AppBuilder();
            Repository = new FakeConfigurationRepository();
            Creator = new FakeHangfireSchemaCreator();
            Monitor = new FakeMonitoringApi();
            Hangfire = new FakeHangfire(Monitor);
            Configuration = new Configuration(Repository, Creator);
            ServerStarter = new ServerStarter(AppBuilder, Hangfire, Repository);
            
            
            Determiner = new WorkerDeterminer(Configuration, Monitor);
        }


        public WorkerDeterminer Determiner { get;}

        public FakeMonitoringApi Monitor { get; }

        public AppBuilder AppBuilder { get; }
        public FakeConfigurationRepository Repository { get; }
        public FakeHangfireSchemaCreator Creator { get; set; }
        public FakeHangfire Hangfire { get; }

        public Configuration Configuration { get; }
        public ServerStarter ServerStarter { get; }
    }
}