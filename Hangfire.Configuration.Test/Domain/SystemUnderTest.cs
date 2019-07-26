using Microsoft.Owin.Builder;

namespace Hangfire.Configuration.Test.Domain
{
    public class SystemUnderTest
    {
        public SystemUnderTest()
        {
            AppBuilder = new AppBuilder();
            Repository = new FakeConfigurationRepository();
            Hangfire = new FakeHangfire();
            Configuration = new Configuration(Repository);
            ServerStarter = new ServerStarter(AppBuilder, Hangfire, Repository);
            Monitor = new FakeMonitoringApi();
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