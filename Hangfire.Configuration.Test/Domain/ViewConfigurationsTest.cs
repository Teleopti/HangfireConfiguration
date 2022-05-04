using System.Linq;
using NUnit.Framework;
using SharpTestsEx;

namespace Hangfire.Configuration.Test.Domain
{
    public class ViewConfigurationsTest
    {
        [Test]
        public void ShouldBuildConfiguration()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration
            {
                Id = 1,
                ConnectionString = "theConnstring",
                SchemaName = "schemaName",
                Active = true
            });

            var result = system.ViewModelBuilder.BuildServerConfigurations().Single();

            Assert.AreEqual(1, result.Id);
            Assert.AreEqual("theConnstring", result.ConnectionString);
            Assert.AreEqual("schemaName", result.SchemaName);
            Assert.AreEqual(true, result.Active);
        }

        [Test]
        public void ShouldBuildConfiguration2()
        {
            var system = new SystemUnderTest();

            system.ConfigurationStorage.Has(new StoredConfiguration
            {
                Id = 2,
                ConnectionString = "Data Source=Server2;Integrated Security=SSPI;Initial Catalog=Test_Database_2;Application Name=Test",
                SchemaName = "schemaName2",
                Active = false
            });

            var result = system.ViewModelBuilder.BuildServerConfigurations().Single();

            Assert.AreEqual(2, result.Id);
            Assert.AreEqual("schemaName2", result.SchemaName);
            Assert.AreEqual(false, result.Active);
        }

        [Test]
        public void ShouldBuildWithNullValues()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration
            {
                Id = 1,
                ConnectionString = null,
                SchemaName = null,
                Active = null,
                GoalWorkerCount = null
            });

            var result = system.ViewModelBuilder.BuildServerConfigurations().Single();

            Assert.AreEqual(1, result.Id);
            Assert.Null(result.SchemaName);
            Assert.Null(result.Active);
            Assert.Null(result.Workers);
        }
        
        [Test]
        public void ShouldBuildForMultipleConfigurations()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration
                {
                    Id = 1,
                    ConnectionString = "Data Source=Server1;Integrated Security=SSPI;Initial Catalog=Test_Database_1;Application Name=Test",
                    SchemaName = "schemaName1",
                    Active = true
                },
                new StoredConfiguration
                {
                    Id = 2,
                    ConnectionString = "Data Source=Server2;Integrated Security=SSPI;Initial Catalog=Test_Database_2;Application Name=Test",
                    SchemaName = "schemaName2",
                    Active = false
                });

            var result = system.ViewModelBuilder.BuildServerConfigurations();

            Assert.AreEqual(2, result.Count());
        }

        [Test]
        public void ShouldBuildWithWorkers()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration {GoalWorkerCount = 10});

            var result = system.ViewModelBuilder.BuildServerConfigurations();

            Assert.AreEqual(10, result.Single().Workers);
        }

        [Test]
        public void ShouldBuildWithDefaultSchemaName()
        {
            var system = new SystemUnderTest();
            var storedConfiguration = new StoredConfiguration
            {
	            ConnectionString = "Data Source=.",
	            SchemaName = null
            };

			system.ConfigurationStorage.Has(storedConfiguration);

            var result = system.ViewModelBuilder.BuildServerConfigurations();

            Assert.AreEqual(DefaultSchemaName.SqlServer(), result.Single().SchemaName);
        }
        
        [Test]
        public void ShouldBuildWithDefaultSchemaNameForPostgres()
        {
	        var system = new SystemUnderTest();
	        var storedConfiguration = new StoredConfiguration
	        {
		        ConnectionString = "Host=localhost",
		        SchemaName = null
	        };

	        system.ConfigurationStorage.Has(storedConfiguration);

	        var result = system.ViewModelBuilder.BuildServerConfigurations();

	        Assert.AreEqual(DefaultSchemaName.Postgres(), result.Single().SchemaName);
        }
	    
        [Test]
        public void ShouldBuildWithDefaultSchemaNameForRedis()
        {
	        var system = new SystemUnderTest();
	        var storedConfiguration = new StoredConfiguration
	        {
		        ConnectionString = "redis-roger",
		        SchemaName = null
	        };

	        system.ConfigurationStorage.Has(storedConfiguration);

	        var result = system.ViewModelBuilder.BuildServerConfigurations();

	        Assert.AreEqual(DefaultSchemaName.Redis(), result.Single().SchemaName);
        }
	    
        [Test]
        public void ShouldBuildWithConfigurationName()
        {
            var system = new SystemUnderTest();
            system.ConfigurationStorage.Has(new StoredConfiguration
            {
                Name = "name"
            });

            var result = system.ViewModelBuilder.BuildServerConfigurations();

            Assert.AreEqual("name", result.Single().Name);
        }
        
        [Test]
        public void ShouldBuildWithMaxWorkersPerServer()
        {
            var system = new SystemUnderTest();
            system.WithMaxWorkersPerServer(5);

            var result = system.ViewModelBuilder.BuildServerConfigurations();

            Assert.AreEqual(5, result.Single().MaxWorkersPerServer);
        }

        [Test]
        public void ShouldHideSqlServerPassword()
        {
	        var system = new SystemUnderTest();
	        system.ConfigurationStorage.Has(new StoredConfiguration
	        {
		        ConnectionString = "Data Source=.;Initial Catalog=foo;User Id=me;Password=thePassword;"
	        });

	        var result = system.ViewModelBuilder.BuildServerConfigurations().Single();

	        result.ConnectionString.Should().Not.Contain("thePassword");
	        result.ConnectionString.Should().Contain("******");
        }

        [Test]
        public void ShouldKeepConnstringAsIsIfSqlServerIntegratedSecurity()
        {
	        var system = new SystemUnderTest();
	        var connstring = "Data Source=.;Initial Catalog=a;Integrated Security=SSPI;";
	        system.ConfigurationStorage.Has(new StoredConfiguration
	        {
		        ConnectionString = connstring
	        });

	        var result = system.ViewModelBuilder.BuildServerConfigurations().Single();

	        result.ConnectionString.Should().Be.EqualTo(connstring);
        }
        
        [Test]
        public void ShouldHidePostgresPassword()
        {
	        var system = new SystemUnderTest();
	        system.ConfigurationStorage.Has(new StoredConfiguration
	        {
		        ConnectionString = "Host=.;Database=foo;User Id=me;Password=thePassword;"
	        });

	        var result = system.ViewModelBuilder.BuildServerConfigurations().Single();

	        result.ConnectionString.Should().Not.Contain("thePassword");
	        result.ConnectionString.Should().Contain("******");
        }
        
        [Test]
        public void ShouldKeepConnstringAsIsIfPostgresIntegratedSecurity()
        {
	        var system = new SystemUnderTest();
	        var connstring = "Host=.;Database=a;Integrated Security=true;";
	        system.ConfigurationStorage.Has(new StoredConfiguration
	        {
		        ConnectionString = connstring
	        });

	        var result = system.ViewModelBuilder.BuildServerConfigurations().Single();

	        result.ConnectionString.Should().Be.EqualTo(connstring);
        }
        
        [Test]
        public void ShouldLeaveRedisConnectionStringAsIsIfNoPassword()
        {
	        var system = new SystemUnderTest();
	        system.ConfigurationStorage.Has(new StoredConfiguration
	        {
		        ConnectionString = "localhost"
	        });

	        var result = system.ViewModelBuilder.BuildServerConfigurations().Single();

	        result.ConnectionString.Should().Be.EqualTo("localhost");
        }
        
        [Test]
        public void ShouldHideRedisPassword()
        {
	        var system = new SystemUnderTest();
	        system.ConfigurationStorage.Has(new StoredConfiguration
	        {
		        ConnectionString = "localhost,password=thePassword"
	        });

	        var result = system.ViewModelBuilder.BuildServerConfigurations().Single();

	        result.ConnectionString.Should().Not.Contain("thePassword");
	        result.ConnectionString.Should().Contain("******");
        }
        
        [Test]
        public void ShouldHideRedisPasswordCasingAndSpaces()
        {
	        var system = new SystemUnderTest();
	        system.ConfigurationStorage.Has(new StoredConfiguration
	        {
		        ConnectionString = "localhost, paSsword=thePassword"
	        });

	        var result = system.ViewModelBuilder.BuildServerConfigurations().Single();

	        result.ConnectionString.Should().Not.Contain("thePassword");
        }
        
        [Test]
        public void ShouldNotReplacePasswordIfStringExistsOnOtherPlacesInConnectionString()
        {
	        var system = new SystemUnderTest();
	        system.ConfigurationStorage.Has(new StoredConfiguration
	        {
		        ConnectionString = "localhost,password=o"
	        });

	        var result = system.ViewModelBuilder.BuildServerConfigurations().Single();

	        result.ConnectionString.Should().StartWith("localhost");
        }
        
        [Test]
        public void ShouldHandleRedisConnstringContainsEqualSign()
        {
	        var system = new SystemUnderTest();
	        system.ConfigurationStorage.Has(new StoredConfiguration
	        {
		        ConnectionString = "myserver,password=thåström=great"
	        });

	        var result = system.ViewModelBuilder.BuildServerConfigurations().Single();

	        result.ConnectionString.Should().Not.Contain("great");
        }
    }
}