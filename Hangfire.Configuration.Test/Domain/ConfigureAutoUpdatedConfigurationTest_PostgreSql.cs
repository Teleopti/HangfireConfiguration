using System.Data.SqlClient;
using System.Linq;
using Hangfire.PostgreSql;
using Hangfire.SqlServer;
using Npgsql;
using Xunit;

namespace Hangfire.Configuration.Test.Domain
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
		                ConnectionString = new NpgsqlConnectionStringBuilder{Host = "host"}.ToString(),
		                Name = DefaultConfigurationName.Name()
	                }
                }
            }, null, (PostgreSqlStorageOptions)null);

            var host = new NpgsqlConnectionStringBuilder(system.ConfigurationStorage.Data.Single().ConnectionString).Host;
            Assert.Equal("host", host);
        }

        [Fact]
        public void ShouldNotConfigureAutoUpdatedServerIfNoneGiven()
        {
            var system = new SystemUnderTest();

            system.WorkerServerStarter.Start(new ConfigurationOptions(), null, (PostgreSqlStorageOptions)null);

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
			            ConnectionString = new NpgsqlConnectionStringBuilder{ ApplicationName = "ApplicationName" }.ToString(),
			            Name = DefaultConfigurationName.Name()
		            }
	            }
            }, null, (PostgreSqlStorageOptions)null);

            var applicationName = new NpgsqlConnectionStringBuilder(system.ConfigurationStorage.Data.Single().ConnectionString).ApplicationName;
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
			            ConnectionString = new NpgsqlConnectionStringBuilder{ Host = "host" }.ToString(),
			            Name = DefaultConfigurationName.Name()
		            }
	            }
            }, null, (PostgreSqlStorageOptions)null);

            var applicationName = new NpgsqlConnectionStringBuilder(system.ConfigurationStorage.Data.Single().ConnectionString).ApplicationName;
            Assert.Equal("Hangfire.AutoUpdate", applicationName);
        }

        [Fact]
        public void ShouldNotUpdateExistingConfigurationThatIsNotMarked()
        {
            var system = new SystemUnderTest();
            var existing = new NpgsqlConnectionStringBuilder() { Host = "existing" }.ToString();
            system.ConfigurationStorage.Has(new StoredConfiguration {ConnectionString = existing});

            system.WorkerServerStarter.Start(new ConfigurationOptions
            {
	            UpdateConfigurations = new []
	            {
		            new UpdateStorageConfiguration
		            {
			            ConnectionString = new NpgsqlConnectionStringBuilder{ Host = "autoupdated" }.ToString(),
			            Name = DefaultConfigurationName.Name()
		            }
	            }
            }, null, (PostgreSqlStorageOptions)null);

            var actual = system.ConfigurationStorage.Data.OrderBy(x => x.Id).First();
            Assert.Equal(existing, actual.ConnectionString);
        }

        [Fact]
        public void ShouldNotUpdateExistingConfigurationThatIsNotMarked2()
        {
            var system = new SystemUnderTest();
            var existing = new NpgsqlConnectionStringBuilder { Host = "AnotherDataSourceWith.AutoUpdate.InIt"}.ToString();
            system.ConfigurationStorage.Has(new StoredConfiguration {ConnectionString = existing});

            system.WorkerServerStarter.Start(new ConfigurationOptions
            {
	            UpdateConfigurations = new []
	            {
		            new UpdateStorageConfiguration
		            {
			            ConnectionString = new NpgsqlConnectionStringBuilder{ Host = "autoupdated" }.ToString(),
			            Name = DefaultConfigurationName.Name()
		            }
	            }
            }, null, (PostgreSqlStorageOptions)null);

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
			            ConnectionString = new NpgsqlConnectionStringBuilder{ Host = "autoupdated" }.ToString(),
			            Name = DefaultConfigurationName.Name()
		            }
	            }
            }, null, (PostgreSqlStorageOptions)null);

            var actual = system.ConfigurationStorage.Data.OrderBy(x => x.Id).First();
            Assert.Equal(existing, actual.ConnectionString);
        }

        [Fact]
        public void ShouldAddAutoUpdatedConfigurationIfNoMarkedExists()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration {ConnectionString = new NpgsqlConnectionStringBuilder { Host = "existing"}.ToString()});

            system.WorkerServerStarter.Start(new ConfigurationOptions
            {
	            UpdateConfigurations = new []
	            {
		            new UpdateStorageConfiguration
		            {
			            ConnectionString = new NpgsqlConnectionStringBuilder{ Host = "autoupdated" }.ToString(),
			            Name = DefaultConfigurationName.Name()
		            }
	            }
            }, null, (PostgreSqlStorageOptions)null);

            var actual = system.ConfigurationStorage.Data.OrderBy(x => x.Id).Last();
            Assert.Equal("autoupdated", new NpgsqlConnectionStringBuilder(actual.ConnectionString).Host);
        }

        [Fact]
        public void ShouldUpdate()
        {
            var system = new SystemUnderTest();
            var existing = new NpgsqlConnectionStringBuilder { Host = "existingDataSource", ApplicationName = "existingApplicationName.AutoUpdate"}.ToString();
            system.ConfigurationStorage.Has(new StoredConfiguration {ConnectionString = existing});

            system.WorkerServerStarter.Start(new ConfigurationOptions
            {
	            UpdateConfigurations = new []
	            {
		            new UpdateStorageConfiguration
		            {
			            ConnectionString = new NpgsqlConnectionStringBuilder{ Host = "newDataSource", ApplicationName = "newApplicationName"}.ToString(),
			            Name = DefaultConfigurationName.Name()
		            }
	            }
            }, null, (PostgreSqlStorageOptions)null);

            var updatedConnectionString = new NpgsqlConnectionStringBuilder(system.ConfigurationStorage.Data.Single().ConnectionString);
            Assert.Equal("newDataSource", updatedConnectionString.Host);
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
			            ConnectionString = new NpgsqlConnectionStringBuilder{ Host = "dataSource", ApplicationName = "applicationName"}.ToString(),
			            Name = DefaultConfigurationName.Name()
		            }
	            }
            }, null, (PostgreSqlStorageOptions)null);

            var expected = new NpgsqlConnectionStringBuilder { Host = "dataSource", ApplicationName = "applicationName.AutoUpdate"}.ToString();
            Assert.Equal(55, system.ConfigurationStorage.Data.Single().GoalWorkerCount);
            Assert.Equal(expected, system.ConfigurationStorage.Data.Single().ConnectionString);
        }

        [Fact]
        public void ShouldUpdateOneOfTwo()
        {
            var system = new SystemUnderTest();
            var one = new NpgsqlConnectionStringBuilder { Host = "One"}.ToString();
            var two = new NpgsqlConnectionStringBuilder { Host = "Two", ApplicationName = "Two.AutoUpdate"}.ToString();
            system.ConfigurationStorage.Has(new StoredConfiguration {ConnectionString = one});
            system.ConfigurationStorage.Has(new StoredConfiguration {ConnectionString = two});

            system.WorkerServerStarter.Start(new ConfigurationOptions
            {
	            UpdateConfigurations = new []
	            {
		            new UpdateStorageConfiguration
		            {
			            ConnectionString = new NpgsqlConnectionStringBuilder{ Host = "UpdatedTwo", ApplicationName = "UpdatedTwo"}.ToString(),
			            Name = DefaultConfigurationName.Name()
		            }
	            }
            }, null, (PostgreSqlStorageOptions)null);

            var expected = new NpgsqlConnectionStringBuilder { Host = "UpdatedTwo", ApplicationName = "UpdatedTwo.AutoUpdate"}.ToString();
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
			            ConnectionString = new NpgsqlConnectionStringBuilder{ Host = "DataSource" }.ToString(),
			            Name = DefaultConfigurationName.Name()
		            }
	            }
            }, null, (PostgreSqlStorageOptions)null);

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
			            ConnectionString = new NpgsqlConnectionStringBuilder{ Host = "DataSource" }.ToString(),
			            Name = DefaultConfigurationName.Name()
		            }
	            }
            }, null, (PostgreSqlStorageOptions)null);

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
			            ConnectionString = new NpgsqlConnectionStringBuilder{ Host = "DataSource" }.ToString(),
			            Name = DefaultConfigurationName.Name()
		            }
	            }
            }, null, (PostgreSqlStorageOptions)null);

            Assert.True(system.ConfigurationStorage.Data.Single().Active);
        }

        [Fact]
        public void ShouldNotActivateWhenUpdating()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration {ConnectionString = new SqlConnectionStringBuilder {ApplicationName = "ApplicationName.AutoUpdate"}.ToString(), Active = false});
            system.ConfigurationStorage.Has(new StoredConfiguration {ConnectionString = new NpgsqlConnectionStringBuilder { Host = "DataSource"}.ToString(), Active = true});

            system.WorkerServerStarter.Start(new ConfigurationOptions
            {
	            UpdateConfigurations = new []
	            {
		            new UpdateStorageConfiguration
		            {
			            ConnectionString = new NpgsqlConnectionStringBuilder{ Host = "AutoUpdate" }.ToString(),
			            Name = DefaultConfigurationName.Name()
		            }
	            }
            }, null, (PostgreSqlStorageOptions)null);

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
			            ConnectionString = new NpgsqlConnectionStringBuilder{ Host = "AutoUpdate" }.ToString(),
			            Name = DefaultConfigurationName.Name(),
			            SchemaName = "schemaName"
		            }
	            }
            }, null, (PostgreSqlStorageOptions)null);

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
			            ConnectionString = new NpgsqlConnectionStringBuilder{ Host = "AutoUpdate" }.ToString(),
			            Name = DefaultConfigurationName.Name(),
			            SchemaName = "schemaName"
		            }
	            }
            }, null, (PostgreSqlStorageOptions)null);

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
			            ConnectionString = new NpgsqlConnectionStringBuilder{ Host = "FirstUpdate" }.ToString(),
			            Name = DefaultConfigurationName.Name()
		            }
	            }
            }, null, (PostgreSqlStorageOptions)null);

            system.PublisherQueries.QueryPublishers(new ConfigurationOptions
            {
	            UpdateConfigurations = new []
	            {
		            new UpdateStorageConfiguration
		            {
			            ConnectionString = new NpgsqlConnectionStringBuilder{ Host = "SecondUpdate" }.ToString(),
			            Name = DefaultConfigurationName.Name()
		            }
	            }
            }, new SqlServerStorageOptions());

            var host = new NpgsqlConnectionStringBuilder(system.ConfigurationStorage.Data.Single().ConnectionString).Host;
            Assert.Equal("FirstUpdate", host);
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
			            ConnectionString = new NpgsqlConnectionStringBuilder{ Host = "FirstUpdate" }.ToString(),
			            Name = DefaultConfigurationName.Name()
		            }
	            }
            }, null, (PostgreSqlStorageOptions)null);
            system.ConfigurationStorage.Data = Enumerable.Empty<StoredConfiguration>();
            
            system.PublisherQueries.QueryPublishers(new ConfigurationOptions
            {
	            UpdateConfigurations = new []
	            {
		            new UpdateStorageConfiguration
		            {
			            ConnectionString = new NpgsqlConnectionStringBuilder{ Host = "SecondUpdate" }.ToString(),
			            Name = DefaultConfigurationName.Name()
		            }
	            }
            }, new SqlServerStorageOptions());

            var host = new NpgsqlConnectionStringBuilder(system.ConfigurationStorage.Data.Single().ConnectionString).Host;
            Assert.Equal("SecondUpdate", host);
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
			            ConnectionString = new NpgsqlConnectionStringBuilder{ Host = "DataSource" }.ToString(),
			            Name = DefaultConfigurationName.Name()
		            }
	            }
            }, null, (PostgreSqlStorageOptions)null);

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
			            ConnectionString = new NpgsqlConnectionStringBuilder{ Host = "DataSource" }.ToString(),
			            Name = DefaultConfigurationName.Name()
		            }
	            }
            }, null, (PostgreSqlStorageOptions)null);

            Assert.Equal("Hangfire", system.ConfigurationStorage.Data.Single().Name);
        }
    }
}