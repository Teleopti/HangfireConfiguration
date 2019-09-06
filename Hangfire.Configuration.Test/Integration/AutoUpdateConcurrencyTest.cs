using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Hangfire.Configuration.Test.Integration
{
    [Collection("NotParallel")]
    public class AutoUpdateConcurrencyTest
    {
        [Fact, CleanDatabase]
        public void ShouldNotInsertMultiple()
        {
            Parallel.ForEach(Enumerable.Range(1, 10), (item) =>
            {
                var connection = ConnectionUtils.GetConnectionString();
                var repository = new ConfigurationRepository(connection);
                var configurator = new ConfigurationAutoUpdater(repository, new DistributedLock("lockid", connection));
                configurator.Update( new ConfigurationOptions
                {
                    AutoUpdatedHangfireConnectionString = connection,
                    AutoUpdatedHangfireSchemaName = "SchemaName"
                });
            });

            Assert.Single(new ConfigurationRepository(ConnectionUtils.GetConnectionString()).ReadConfigurations());
        }        
    }
}