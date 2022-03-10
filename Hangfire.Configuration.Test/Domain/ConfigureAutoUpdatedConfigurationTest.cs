using System.Data.SqlClient;
using System.Linq;
using Hangfire.SqlServer;
using NUnit.Framework;

namespace Hangfire.Configuration.Test.Domain
{
    public class ConfigureAutoUpdatedConfigurationTest
    {
        [Test]
        public void ShouldConfigureAutoUpdatedServer()
        {
            var system = new SystemUnderTest();

            system.WorkerServerStarter.Start(new ConfigurationOptions
            {
	            UpdateConfigurations = new []
	            {
		            new UpdateStorageConfiguration
		            {
			            ConnectionString = new SqlConnectionStringBuilder{ DataSource = "DataSource" }.ToString(),
			            Name = DefaultConfigurationName.Name()
		            }
	            }
            }, null, (SqlServerStorageOptions)null);

            var dataSource = new SqlConnectionStringBuilder(system.ConfigurationStorage.Data.Single().ConnectionString).DataSource;
            Assert.AreEqual("DataSource", dataSource);
        }

        [Test]
        public void ShouldNotConfigureAutoUpdatedServerIfNoneGiven()
        {
            var system = new SystemUnderTest();

            system.WorkerServerStarter.Start(new ConfigurationOptions(), null, (SqlServerStorageOptions)null);

            Assert.IsEmpty(system.ConfigurationStorage.Data);
        }

        [Test]
        public void ShouldMarkAutoUpdatedConnectionString()
        {
            var system = new SystemUnderTest();

            system.WorkerServerStarter.Start(new ConfigurationOptions
            {
	            UpdateConfigurations = new []
	            {
		            new UpdateStorageConfiguration
		            {
			            ConnectionString = new SqlConnectionStringBuilder{ ApplicationName = "ApplicationName" }.ToString(),
			            Name = DefaultConfigurationName.Name()
		            }
	            }
            }, null, (SqlServerStorageOptions)null);

            var applicationName = new SqlConnectionStringBuilder(system.ConfigurationStorage.Data.Single().ConnectionString).ApplicationName;
            Assert.AreEqual("ApplicationName.AutoUpdate", applicationName);
        }

        [Test]
        public void ShouldMarkAutoUpdatedConnectionStringWhenNoApplicationName()
        {
            var system = new SystemUnderTest();

            system.WorkerServerStarter.Start(new ConfigurationOptions
            {
	            UpdateConfigurations = new []
	            {
		            new UpdateStorageConfiguration
		            {
			            ConnectionString = new SqlConnectionStringBuilder{ DataSource = "DataSource" }.ToString(),
			            Name = DefaultConfigurationName.Name()
		            }
	            }
            }, null, (SqlServerStorageOptions)null);

            var applicationName = new SqlConnectionStringBuilder(system.ConfigurationStorage.Data.Single().ConnectionString).ApplicationName;
            Assert.AreEqual("Hangfire.AutoUpdate", applicationName);
        }

        [Test]
        public void ShouldNotUpdateExistingConfigurationThatIsNotMarked()
        {
            var system = new SystemUnderTest();
            var existing = new SqlConnectionStringBuilder {DataSource = "existing"}.ToString();
            system.ConfigurationStorage.Has(new StoredConfiguration {ConnectionString = existing});

            system.WorkerServerStarter.Start(new ConfigurationOptions
            {
	            UpdateConfigurations = new []
	            {
		            new UpdateStorageConfiguration
		            {
			            ConnectionString = new SqlConnectionStringBuilder{ DataSource = "autoupdated" }.ToString(),
			            Name = DefaultConfigurationName.Name()
		            }
	            }
            }, null, (SqlServerStorageOptions)null);

            var actual = system.ConfigurationStorage.Data.OrderBy(x => x.Id).First();
            Assert.AreEqual(existing, actual.ConnectionString);
        }

        [Test]
        public void ShouldNotUpdateExistingConfigurationThatIsNotMarked2()
        {
            var system = new SystemUnderTest();
            var existing = new SqlConnectionStringBuilder {DataSource = "AnotherDataSourceWith.AutoUpdate.InIt"}.ToString();
            system.ConfigurationStorage.Has(new StoredConfiguration {ConnectionString = existing});

            system.WorkerServerStarter.Start(new ConfigurationOptions
            {
	            UpdateConfigurations = new []
	            {
		            new UpdateStorageConfiguration
		            {
			            ConnectionString = new SqlConnectionStringBuilder{ DataSource = "autoupdated" }.ToString(),
			            Name = DefaultConfigurationName.Name()
		            }
	            }
            }, null, (SqlServerStorageOptions)null);

            var actual = system.ConfigurationStorage.Data.OrderBy(x => x.Id).First();
            Assert.AreEqual(existing, actual.ConnectionString);
        }

        [Test]
        public void ShouldNotUpdateExistingConfigurationThatIsNotMarked3()
        {
            var system = new SystemUnderTest();
            var existing = new SqlConnectionStringBuilder {ApplicationName = "ExistingApplicationWith.AutoUpdate.InIt"}.ToString();
            system.ConfigurationStorage.Has(new StoredConfiguration {ConnectionString = existing});

            system.WorkerServerStarter.Start(new ConfigurationOptions
            {
	            UpdateConfigurations = new []
	            {
		            new UpdateStorageConfiguration
		            {
			            ConnectionString = new SqlConnectionStringBuilder{ DataSource = "autoupdated" }.ToString(),
			            Name = DefaultConfigurationName.Name()
		            }
	            }
            }, null, (SqlServerStorageOptions)null);

            var actual = system.ConfigurationStorage.Data.OrderBy(x => x.Id).First();
            Assert.AreEqual(existing, actual.ConnectionString);
        }

        [Test]
        public void ShouldAddAutoUpdatedConfigurationIfNoMarkedExists()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration {ConnectionString = new SqlConnectionStringBuilder {DataSource = "existing"}.ToString()});

            system.WorkerServerStarter.Start(new ConfigurationOptions
            {
	            UpdateConfigurations = new []
	            {
		            new UpdateStorageConfiguration
		            {
			            ConnectionString = new SqlConnectionStringBuilder{ DataSource = "autoupdated" }.ToString(),
			            Name = DefaultConfigurationName.Name()
		            }
	            }
            }, null, (SqlServerStorageOptions)null);

            var actual = system.ConfigurationStorage.Data.OrderBy(x => x.Id).Last();
            Assert.AreEqual("autoupdated", new SqlConnectionStringBuilder(actual.ConnectionString).DataSource);
        }

        [Test]
        public void ShouldUpdate()
        {
            var system = new SystemUnderTest();
            var existing = new SqlConnectionStringBuilder {DataSource = "existingDataSource", ApplicationName = "existingApplicationName.AutoUpdate"}.ToString();
            system.ConfigurationStorage.Has(new StoredConfiguration {ConnectionString = existing});

            system.WorkerServerStarter.Start(new ConfigurationOptions
            {
	            UpdateConfigurations = new []
	            {
		            new UpdateStorageConfiguration
		            {
			            ConnectionString = new SqlConnectionStringBuilder{ DataSource = "newDataSource", ApplicationName = "newApplicationName"}.ToString(),
			            Name = DefaultConfigurationName.Name()
		            }
	            }
           }, null, (SqlServerStorageOptions)null);

            var updatedConnectionString = new SqlConnectionStringBuilder(system.ConfigurationStorage.Data.Single().ConnectionString);
            Assert.AreEqual("newDataSource", updatedConnectionString.DataSource);
            Assert.AreEqual("newApplicationName.AutoUpdate", updatedConnectionString.ApplicationName);
        }

        [Test]
        public void ShouldUpdateLegacyConfiguration()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration {GoalWorkerCount = 55});

            system.WorkerServerStarter.Start(new ConfigurationOptions
            {
	            UpdateConfigurations = new []
	            {
		            new UpdateStorageConfiguration
		            {
			            ConnectionString = new SqlConnectionStringBuilder{ DataSource = "dataSource", ApplicationName = "applicationName"}.ToString(),
			            Name = DefaultConfigurationName.Name()
		            }
	            }
            }, null, (SqlServerStorageOptions)null);

            var expected = new SqlConnectionStringBuilder {DataSource = "dataSource", ApplicationName = "applicationName.AutoUpdate"}.ToString();
            Assert.AreEqual(55, system.ConfigurationStorage.Data.Single().GoalWorkerCount);
            Assert.AreEqual(expected, system.ConfigurationStorage.Data.Single().ConnectionString);
        }

        [Test]
        public void ShouldUpdateOneOfTwo()
        {
            var system = new SystemUnderTest();
            var one = new SqlConnectionStringBuilder {DataSource = "One"}.ToString();
            var two = new SqlConnectionStringBuilder {DataSource = "Two", ApplicationName = "Two.AutoUpdate"}.ToString();
            system.ConfigurationStorage.Has(new StoredConfiguration {ConnectionString = one});
            system.ConfigurationStorage.Has(new StoredConfiguration {ConnectionString = two});

            system.WorkerServerStarter.Start(new ConfigurationOptions
            {
	            UpdateConfigurations = new []
	            {
		            new UpdateStorageConfiguration
		            {
			            ConnectionString = new SqlConnectionStringBuilder{ DataSource = "UpdatedTwo", ApplicationName = "UpdatedTwo"}.ToString(),
			            Name = DefaultConfigurationName.Name()
		            }
	            }
            }, null, (SqlServerStorageOptions)null);

            var expected = new SqlConnectionStringBuilder {DataSource = "UpdatedTwo", ApplicationName = "UpdatedTwo.AutoUpdate"}.ToString();
            Assert.AreEqual(expected, system.ConfigurationStorage.Data.Last().ConnectionString);
        }

        [Test]
        public void ShouldActivateOnFirstUpdate()
        {
            var system = new SystemUnderTest();

            system.WorkerServerStarter.Start(new ConfigurationOptions
            {
	            UpdateConfigurations = new []
	            {
		            new UpdateStorageConfiguration
		            {
			            ConnectionString = new SqlConnectionStringBuilder{ DataSource = "DataSource" }.ToString(),
			            Name = DefaultConfigurationName.Name()
		            }
	            }
            }, null, (SqlServerStorageOptions)null);

            Assert.True(system.ConfigurationStorage.Data.Single().Active);
        }

        [Test]
        public void ShouldActivateLegacyConfigurationWhenConfiguredAsDefault()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration {GoalWorkerCount = 4});

            system.WorkerServerStarter.Start(new ConfigurationOptions
            {
	            UpdateConfigurations = new []
	            {
		            new UpdateStorageConfiguration
		            {
			            ConnectionString = new SqlConnectionStringBuilder{ DataSource = "DataSource" }.ToString(),
			            Name = DefaultConfigurationName.Name()
		            }
	            }
            }, null, (SqlServerStorageOptions)null);

            Assert.True(system.ConfigurationStorage.Data.Single().Active);
        }

        [Test]
        public void ShouldBeActiveOnUpdateIfActiveBefore()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration
            {
                ConnectionString = new SqlConnectionStringBuilder {ApplicationName = "ApplicationName.AutoUpdate"}.ToString(),
                Active = true
            });

            system.WorkerServerStarter.Start(new ConfigurationOptions
            {
	            UpdateConfigurations = new []
	            {
		            new UpdateStorageConfiguration
		            {
			            ConnectionString = new SqlConnectionStringBuilder{ DataSource = "DataSource" }.ToString(),
			            Name = DefaultConfigurationName.Name()
		            }
	            }
            }, null, (SqlServerStorageOptions)null);

            Assert.True(system.ConfigurationStorage.Data.Single().Active);
        }

        [Test]
        public void ShouldNotActivateWhenUpdating()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration {ConnectionString = new SqlConnectionStringBuilder {ApplicationName = "ApplicationName.AutoUpdate"}.ToString(), Active = false});
            system.ConfigurationStorage.Has(new StoredConfiguration {ConnectionString = new SqlConnectionStringBuilder {DataSource = "DataSource"}.ToString(), Active = true});

            system.WorkerServerStarter.Start(new ConfigurationOptions
            {
	            UpdateConfigurations = new []
	            {
		            new UpdateStorageConfiguration
		            {
			            ConnectionString = new SqlConnectionStringBuilder{ DataSource = "AutoUpdate" }.ToString(),
			            Name = DefaultConfigurationName.Name()
		            }
	            }
            }, null, (SqlServerStorageOptions)null);

            Assert.False(system.ConfigurationStorage.Data.First().Active);
            Assert.True(system.ConfigurationStorage.Data.Last().Active);
        }

        [Test]
        public void ShouldSaveSchemaName()
        {
            var system = new SystemUnderTest();

            system.WorkerServerStarter.Start(new ConfigurationOptions
            {
	            UpdateConfigurations = new []
	            {
		            new UpdateStorageConfiguration
		            {
			            ConnectionString = new SqlConnectionStringBuilder{ DataSource = "AutoUpdate" }.ToString(),
			            Name = DefaultConfigurationName.Name(),
			            SchemaName = "schemaName"
		            }
	            }
            }, null, (SqlServerStorageOptions)null);

            Assert.AreEqual("schemaName", system.ConfigurationStorage.Data.Single().SchemaName);
        }

        [Test]
        public void ShouldSaveSchemaNameOnLegacyConfiguration()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration {GoalWorkerCount = 4});

            system.WorkerServerStarter.Start(new ConfigurationOptions
            {
	            UpdateConfigurations = new []
	            {
		            new UpdateStorageConfiguration
		            {
			            ConnectionString = new SqlConnectionStringBuilder{ DataSource = "AutoUpdate" }.ToString(),
			            Name = DefaultConfigurationName.Name(),
			            SchemaName = "schemaName"
		            }
	            }
            }, null, (SqlServerStorageOptions)null);

            Assert.AreEqual("schemaName", system.ConfigurationStorage.Data.Single().SchemaName);
        }

        [Test]
        public void ShouldOnlyAutoUpdateOnce()
        {
            var system = new SystemUnderTest();
            system.WorkerServerStarter.Start(new ConfigurationOptions
            {
	            UpdateConfigurations = new []
	            {
		            new UpdateStorageConfiguration
		            {
			            ConnectionString = new SqlConnectionStringBuilder{ DataSource = "FirstUpdate" }.ToString(),
			            Name = DefaultConfigurationName.Name()
		            }
	            }
            }, null, (SqlServerStorageOptions)null);

            system.PublisherQueries.QueryPublishers(new ConfigurationOptions
            {
	            UpdateConfigurations = new []
	            {
		            new UpdateStorageConfiguration
		            {
			            ConnectionString = new SqlConnectionStringBuilder{ DataSource = "SecondUpdate" }.ToString(),
			            Name = DefaultConfigurationName.Name()
		            }
	            }
            }, new SqlServerStorageOptions());

            var dataSource = new SqlConnectionStringBuilder(system.ConfigurationStorage.Data.Single().ConnectionString).DataSource;
            Assert.AreEqual("FirstUpdate", dataSource);
        }

        [Test]
        public void ShouldAutoUpdateTwiceIfAllConfigurationsWhereRemoved()
        {
            var system = new SystemUnderTest();
            system.WorkerServerStarter.Start(new ConfigurationOptions
            {
	            UpdateConfigurations = new []
	            {
		            new UpdateStorageConfiguration
		            {
			            ConnectionString = new SqlConnectionStringBuilder{ DataSource = "FirstUpdate" }.ToString(),
			            Name = DefaultConfigurationName.Name()
		            }
	            }
            }, null, (SqlServerStorageOptions)null);
            system.ConfigurationStorage.Data = Enumerable.Empty<StoredConfiguration>();
            
            system.PublisherQueries.QueryPublishers(new ConfigurationOptions
            {
	            UpdateConfigurations = new []
	            {
		            new UpdateStorageConfiguration
		            {
			            ConnectionString = new SqlConnectionStringBuilder{ DataSource = "SecondUpdate" }.ToString(),
			            Name = DefaultConfigurationName.Name()
		            }
	            }
            }, new SqlServerStorageOptions());

            var dataSource = new SqlConnectionStringBuilder(system.ConfigurationStorage.Data.Single().ConnectionString).DataSource;
            Assert.AreEqual("SecondUpdate", dataSource);
        }

        [Test]
        public void ShouldAutoUpdateWithDefaultConfigurationName()
        {
            var system = new SystemUnderTest();

            system.WorkerServerStarter.Start(new ConfigurationOptions
            {
	            UpdateConfigurations = new []
	            {
		            new UpdateStorageConfiguration
		            {
			            ConnectionString = new SqlConnectionStringBuilder{ DataSource = "DataSource" }.ToString(),
			            Name = DefaultConfigurationName.Name()
		            }
	            }
            }, null, (SqlServerStorageOptions)null);

            Assert.AreEqual("Hangfire", system.ConfigurationStorage.Data.Single().Name);
        }

        [Test]
        public void ShouldAutoUpdateWithDefaultConfigurationName2()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration {ConnectionString = new SqlConnectionStringBuilder {ApplicationName = "ApplicationName.AutoUpdate"}.ToString(), Active = false});

            system.WorkerServerStarter.Start(new ConfigurationOptions
            {
	            UpdateConfigurations = new []
	            {
		            new UpdateStorageConfiguration
		            {
			            ConnectionString = new SqlConnectionStringBuilder{ DataSource = "DataSource" }.ToString(),
			            Name = DefaultConfigurationName.Name()
		            }
	            }
            }, null, (SqlServerStorageOptions)null);

            Assert.AreEqual("Hangfire", system.ConfigurationStorage.Data.Single().Name);
        }
    }
}