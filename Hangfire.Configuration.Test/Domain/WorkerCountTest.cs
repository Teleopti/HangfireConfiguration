using System.Linq;
using Hangfire.Configuration.Test.Domain.Fake;
using NUnit.Framework;

namespace Hangfire.Configuration.Test.Domain
{
    public class WorkerCountTest
    {
        [Test]
        public void ShouldGetDefaultForFirstServer()
        {
            var system = new SystemUnderTest();

            system.WorkerServerStarter.Start(
                new ConfigurationOptions
                {
	                UpdateConfigurations = new []
	                {
		                new UpdateStorageConfiguration
		                {
			                Name = DefaultConfigurationName.Name()
		                }
	                }
                });

            Assert.AreEqual(10, system.Hangfire.StartedServers.Single().options.WorkerCount);
        }

        [Test]
        public void ShouldGetGoalForFirstServer()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.HasGoalWorkerCount(8);

            system.WorkerServerStarter.Start();

            Assert.AreEqual(8, system.Hangfire.StartedServers.Single().options.WorkerCount);
        }

        [Test]
        public void ShouldGetOneIfGoalIsZero()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.HasGoalWorkerCount(0);

            system.WorkerServerStarter.Start();

            Assert.AreEqual(1, system.Hangfire.StartedServers.Single().options.WorkerCount);
        }

        [Test]
        public void ShouldGetOneIfGoalIsNegative()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.HasGoalWorkerCount(-1);

            system.WorkerServerStarter.Start();

            Assert.AreEqual(1, system.Hangfire.StartedServers.Single().options.WorkerCount);
        }

        [Test]
        public void ShouldGetMaxOneHundred()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.HasGoalWorkerCount(101);

            system.WorkerServerStarter.Start();

            Assert.AreEqual(100, system.Hangfire.StartedServers.Single().options.WorkerCount);
        }

        [Test]
        public void ShouldUseDefaultGoalWorkerCount()
        {
            var system = new SystemUnderTest();

            system.WorkerServerStarter.Start(new ConfigurationOptionsForTest
            {
	            UpdateConfigurations = new[]
	            {
		            new UpdateStorageConfiguration
		            {
			            // ConnectionString = new SqlConnectionStringBuilder{ DataSource = "Hangfire" }.ToString(),
			            Name = DefaultConfigurationName.Name()
		            }
	            },
	            DefaultGoalWorkerCount = 12
            });

            Assert.AreEqual(12, system.Hangfire.StartedServers.Single().options.WorkerCount);
        }

        [Test]
        public void ShouldUseMinimumWorkerCount()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.HasGoalWorkerCount(0);

            system.WorkerServerStarter.Start(new ConfigurationOptionsForTest
            {
	            MinimumWorkerCount = 2,
            });

            Assert.AreEqual(2, system.Hangfire.StartedServers.Single().options.WorkerCount);
        }

        [Test]
        public void ShouldUseMaximumGoalWorkerCount()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.HasGoalWorkerCount(202);

            system.WorkerServerStarter.Start(new ConfigurationOptionsForTest
            {
	            MaximumGoalWorkerCount = 200,
            });

            Assert.AreEqual(200, system.Hangfire.StartedServers.Single().options.WorkerCount);
        }

        [Test]
        public void ShouldUseMinimumServerCount()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.HasGoalWorkerCount(15);

            system.WorkerServerStarter.Start(new ConfigurationOptionsForTest
            {
	            MinimumServerCount = 3,
            });

            Assert.AreEqual(15 / 3, system.Hangfire.StartedServers.Single().options.WorkerCount);
        }

        [Test]
        public void ShouldUseMinimumWorkerCountWithMinimumKnownServers()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.HasGoalWorkerCount(7);

            system.WorkerServerStarter.Start(new ConfigurationOptionsForTest
            {
	            MinimumServerCount = 2,
	            MinimumWorkerCount = 6,
            });

            Assert.AreEqual(6, system.Hangfire.StartedServers.Single().options.WorkerCount);
        }

        [Test]
        public void ShouldGetGoalForFirstServerWhenMinimumKnownServersIsZero()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.HasGoalWorkerCount(8);

            system.WorkerServerStarter.Start(
                new ConfigurationOptionsForTest
                {
                    MinimumServerCount = 0,
                });
            
            Assert.AreEqual(8, system.Hangfire.StartedServers.Single().options.WorkerCount);
        }

        [Test]
        public void ShouldDisableWorkerDeterminer()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.HasGoalWorkerCount(8);

            system.UseOptions(new ConfigurationOptions {UseWorkerDeterminer = false});
            system.UseServerOptions(new BackgroundJobServerOptions {WorkerCount = 52});
            system.WorkerServerStarter.Start();

            Assert.AreEqual(52, system.Hangfire.StartedServers.Single().options.WorkerCount);
        }
        
        [Test]
        public void ShouldGetHalfOfDefaultForFirstServer()
        {
            var system = new SystemUnderTest();

            system.WorkerServerStarter.Start(
                new ConfigurationOptionsForTest
                {
                    MinimumServerCount = 2,
                    UpdateConfigurations = new []
                    {
	                    new UpdateStorageConfiguration
	                    {
		                    Name = DefaultConfigurationName.Name()
	                    }
                    }
                });
            
            Assert.AreEqual(5, system.Hangfire.StartedServers.Single().options.WorkerCount);
        }

        [Test]
        public void ShouldGetHalfOfGoalForFirstServer()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.HasGoalWorkerCount(8);

            system.WorkerServerStarter.Start(new ConfigurationOptionsForTest
            {
                MinimumServerCount = 2,
			});

            Assert.AreEqual(4, system.Hangfire.StartedServers.Single().options.WorkerCount);
        }

        [Test]
        public void ShouldGetHalfOfGoalOnRestartOfSingleServer()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.HasGoalWorkerCount(8);
            system.Monitor.AnnounceServer("restartedServer");

            system.WorkerServerStarter.Start(new ConfigurationOptionsForTest
            {
                MinimumServerCount = 2,
			});

            Assert.AreEqual(4, system.Hangfire.StartedServers.Single().options.WorkerCount);
        }
        
        [Test]
        public void ShouldGetHalfOfMaxOneHundred()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.HasGoalWorkerCount(101);

            system.WorkerServerStarter.Start(new ConfigurationOptionsForTest
            {
                MinimumServerCount = 2,
			});

            Assert.AreEqual(50, system.Hangfire.StartedServers.Single().options.WorkerCount);
        }

        [Test]
        public void ShouldUseDefaultGoalWorkerCountWithMinimumKnownServers()
        {
            var system = new SystemUnderTest();

            system.WorkerServerStarter.Start(new ConfigurationOptionsForTest
            {
	            UpdateConfigurations = new []
	            {
		            new UpdateStorageConfiguration
		            {
			            Name = DefaultConfigurationName.Name()
		            }
	            },
	            DefaultGoalWorkerCount = 12,
                MinimumServerCount = 2
            });

            Assert.AreEqual(6, system.Hangfire.StartedServers.Single().options.WorkerCount);
        }
        
    }
}