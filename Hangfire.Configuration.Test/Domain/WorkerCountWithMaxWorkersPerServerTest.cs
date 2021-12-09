using System.Linq;
using Xunit;

namespace Hangfire.Configuration.Test.Domain
{
    public class WorkerCountWithMaxWorkersPerServerTest
    {
        [Fact]
        public void ShouldUseMaxWorkersPerServer()
        {
            var system = new SystemUnderTest();
            system
	            .WithGoalWorkerCount(10)
	            .WithMaxWorkersPerServer(2);
            
            system.StartWorkerServer();

            Assert.Equal(2, system.Hangfire.StartedServers.Single().options.WorkerCount);
        }
        
        [Fact]
        public void ShouldUseMinimumWhenMaxIsLess()
        {
            var system = new SystemUnderTest();
            var options = new ConfigurationOptions();
            options.ConnectionString = ConnectionUtils.GetFakeConnectionString();

			options.WorkerDeterminerOptions.MinimumWorkerCount = 2;
            system
	            .WithGoalWorkerCount(10)
	            .WithOptions(options)
	            .WithMaxWorkersPerServer(1);
            
            system.StartWorkerServer();

            Assert.Equal(2, system.Hangfire.StartedServers.Single().options.WorkerCount);
        }
    }
}