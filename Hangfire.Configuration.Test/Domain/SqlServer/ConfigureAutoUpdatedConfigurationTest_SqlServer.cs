using System.Data.SqlClient;
using System.Linq;
using Hangfire.SqlServer;
using Xunit;

namespace Hangfire.Configuration.Test.Domain.SqlServer
{
    public class ConfigureAutoUpdatedConfigurationTest
    {
        [Fact]
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
            Assert.Equal("DataSource", dataSource);
        }

        [Fact]
        public void ShouldNotConfigureAutoUpdatedServerIfNoneGiven()
        {
            var system = new SystemUnderTest();

            system.WorkerServerStarter.Start(new ConfigurationOptions(), null, (SqlServerStorageOptions)null);

            Assert.Empty(system.ConfigurationStorage.Data);
        }

        [Fact]
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
            Assert.Equal("ApplicationName.AutoUpdate", applicationName);
        }

        [Fact]
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
            Assert.Equal("Hangfire.AutoUpdate", applicationName);
        }

        [Fact]
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
            Assert.Equal(existing, actual.ConnectionString);
        }

        [Fact]
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
            Assert.Equal(existing, actual.ConnectionString);
        }

        [Fact]
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
            Assert.Equal(existing, actual.ConnectionString);
        }

        [Fact]
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
            Assert.Equal("autoupdated", new SqlConnectionStringBuilder(actual.ConnectionString).DataSource);
        }

        [Fact]
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
            Assert.Equal("newDataSource", updatedConnectionString.DataSource);
            Assert.Equal("newApplicationName.AutoUpdate", updatedConnectionString.ApplicationName);
        }

        [Fact]
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
            Assert.Equal(55, system.ConfigurationStorage.Data.Single().GoalWorkerCount);
            Assert.Equal(expected, system.ConfigurationStorage.Data.Single().ConnectionString);
        }

        [Fact]
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
            Assert.Equal(expected, system.ConfigurationStorage.Data.Last().ConnectionString);
        }

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

            Assert.Equal("schemaName", system.ConfigurationStorage.Data.Single().SchemaName);
        }

        [Fact]
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

            Assert.Equal("schemaName", system.ConfigurationStorage.Data.Single().SchemaName);
        }

        [Fact]
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
            Assert.Equal("FirstUpdate", dataSource);
        }

        [Fact]
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
            Assert.Equal("SecondUpdate", dataSource);
        }

        [Fact]
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

            Assert.Equal("Hangfire", system.ConfigurationStorage.Data.Single().Name);
        }

        [Fact]
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

            Assert.Equal("Hangfire", system.ConfigurationStorage.Data.Single().Name);
        }
    }
}