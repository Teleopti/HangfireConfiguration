using Microsoft.Owin.Builder;

namespace Hangfire.Configuration.Test.Domain
{
    public class SystemUnderTest
    {
        public SystemUnderTest()
        {
            AppBuilder = new AppBuilder();
            Repository = new FakeConfigurationRepository();
            Monitor = new FakeMonitoringApi();
            Hangfire = new FakeHangfire(Monitor);
            Configuration = new Configuration(Repository);
            ServerStarter = new ServerStarter(AppBuilder, Hangfire, Repository);
            
            
            Determiner = new WorkerDeterminer(Configuration, Monitor);
        }

        public WorkerDeterminer Determiner { get;}

        public FakeMonitoringApi Monitor { get; }

        public AppBuilder AppBuilder { get; }
        public FakeConfigurationRepository Repository { get; }
        public FakeHangfire Hangfire { get; }

        public Configuration Configuration { get; }
        public ServerStarter ServerStarter { get; }
    }
}