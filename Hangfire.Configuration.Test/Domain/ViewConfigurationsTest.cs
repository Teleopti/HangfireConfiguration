using System.Linq;
using NUnit.Framework;
using SharpTestsEx;

namespace Hangfire.Configuration.Test.Domain;

public class ViewConfigurationsTest
{
	[Test]
	public void ShouldBuildConfiguration()
	{
		var system = new SystemUnderTest();
		system.WithConfiguration(new StoredConfiguration
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

		system.WithConfiguration(new StoredConfiguration
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
		system.WithConfiguration(new StoredConfiguration
		{
			Id = 1,
			ConnectionString = null,
			SchemaName = null,
			Active = null,
			Containers = [new ContainerConfiguration {GoalWorkerCount = null}]
		});

		var result = system.ViewModelBuilder.BuildServerConfigurations().Single();

		Assert.AreEqual(1, result.Id);
		Assert.Null(result.SchemaName);
		Assert.IsTrue(result.Active);
		Assert.Null(result.Containers[0].Workers);
	}

	[Test]
	public void ShouldBuildForMultipleConfigurations()
	{
		var system = new SystemUnderTest();
		system.WithConfiguration(new StoredConfiguration
		{
			Id = 1,
			ConnectionString = "Data Source=Server1;Integrated Security=SSPI;Initial Catalog=Test_Database_1;Application Name=Test",
			SchemaName = "schemaName1",
			Active = true
		});
		system.WithConfiguration(new StoredConfiguration
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
		system.WithConfiguration(new StoredConfiguration
		{
			Containers = [new ContainerConfiguration {GoalWorkerCount = 10}]
		});

		var result = system.ViewModelBuilder.BuildServerConfigurations();

		Assert.AreEqual(10, result.Single().Containers[0].Workers);
	}

	[Test]
	public void ShouldBuildWithDefaultSchemaName()
	{
		var system = new SystemUnderTest();
		system.WithConfiguration(new StoredConfiguration
		{
			ConnectionString = "Data Source=.",
			SchemaName = null
		});

		var result = system.ViewModelBuilder.BuildServerConfigurations();

		Assert.AreEqual(DefaultSchemaName.SqlServer(), result.Single().SchemaName);
	}

	[Test]
	public void ShouldBuildWithDefaultSchemaNameForPostgres()
	{
		var system = new SystemUnderTest();
		system.WithConfiguration(new StoredConfiguration
		{
			ConnectionString = "Host=localhost",
			SchemaName = null
		});

		var result = system.ViewModelBuilder.BuildServerConfigurations();

		Assert.AreEqual(DefaultSchemaName.Postgres(), result.Single().SchemaName);
	}

	[Test]
	public void ShouldBuildWithDefaultSchemaNameForRedis()
	{
		var system = new SystemUnderTest();
		system.WithConfiguration(new StoredConfiguration
		{
			ConnectionString = "redis-roger",
			SchemaName = null
		});

		var result = system.ViewModelBuilder.BuildServerConfigurations();

		Assert.AreEqual(DefaultSchemaName.Redis(), result.Single().SchemaName);
	}

	[Test]
	public void ShouldBuildWithConfigurationName()
	{
		var system = new SystemUnderTest();
		system.WithConfiguration(new StoredConfiguration
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

		Assert.AreEqual(5, result.Single().Containers[0].MaxWorkersPerServer);
	}

	[Test]
	public void ShouldHideSqlServerPassword()
	{
		var system = new SystemUnderTest();
		system.WithConfiguration(new StoredConfiguration
		{
			ConnectionString = "Data Source=.;Initial Catalog=foo;User Id=me;Password=thePassword;"
		});

		var result = system.ViewModelBuilder.BuildServerConfigurations().Single();

		result.ConnectionString.Should().Not.Contain("thePassword");
		result.ConnectionString.Should().Contain("******");
	}

	[Test]
	public void ShouldKeepConnectionStringAsIsIfSqlServerIntegratedSecurity()
	{
		var system = new SystemUnderTest();
		var connectionString = "Data Source=.;Initial Catalog=a;Integrated Security=SSPI;";
		system.WithConfiguration(new StoredConfiguration
		{
			ConnectionString = connectionString
		});

		var result = system.ViewModelBuilder.BuildServerConfigurations().Single();

		result.ConnectionString.Should().Be.EqualTo(connectionString);
	}

	[Test]
	public void ShouldHidePostgresPassword()
	{
		var system = new SystemUnderTest();
		system.WithConfiguration(new StoredConfiguration
		{
			ConnectionString = "Host=.;Database=foo;User Id=me;Password=thePassword;"
		});

		var result = system.ViewModelBuilder.BuildServerConfigurations().Single();

		result.ConnectionString.Should().Not.Contain("thePassword");
		result.ConnectionString.Should().Contain("******");
	}

	[Test]
	public void ShouldLeaveRedisConnectionStringAsIsIfNoPassword()
	{
		var system = new SystemUnderTest();
		system.WithConfiguration(new StoredConfiguration
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
		system.WithConfiguration(new StoredConfiguration
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
		system.WithConfiguration(new StoredConfiguration
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
		system.WithConfiguration(new StoredConfiguration
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
		system.WithConfiguration(new StoredConfiguration
		{
			ConnectionString = "myserver,password=thåström=great"
		});

		var result = system.ViewModelBuilder.BuildServerConfigurations().Single();

		result.ConnectionString.Should().Not.Contain("great");
	}

	[Test]
	public void ShouldBuildWithWorkerBalancerEnabled()
	{
		var system = new SystemUnderTest();
		system.WithConfiguration(new StoredConfiguration
		{
			Containers = [new ContainerConfiguration {WorkerBalancerEnabled = true}]
		});

		var result = system.ViewModelBuilder.BuildServerConfigurations().Single();

		result.Containers[0].WorkerBalancerEnabled.Should().Be.True();
	}

	[Test]
	public void ShouldBuildWithTag()
	{
		var system = new SystemUnderTest();
		system.WithConfiguration(new StoredConfiguration
		{
			Containers = [new ContainerConfiguration {Tag = "my-tag"}]
		});

		var result = system.ViewModelBuilder.BuildServerConfigurations().Single();

		result.Containers[0].Tag.Should().Be("my-tag");
	}

	[Test]
	public void ShouldBuildWithQueues()
	{
		var system = new SystemUnderTest();
		system.UseServerOptions(new BackgroundJobServerOptions
		{
			Queues = ["queue1", "queue2"]
		});
		system.WithConfiguration(new StoredConfiguration
		{
			Containers =
			[
				new ContainerConfiguration
				{
					Queues = ["queue1", "queue2"]
				}
			]
		});

		var result = system.ViewModelBuilder.BuildServerConfigurations().Single();

		result.Containers[0].Queues.Should().Have.SameSequenceAs(["queue1", "queue2"]);
	}

	[Test]
	public void ShouldBuildQueuesWithAppliedLogicForDefaultContainer()
	{
		var system = new SystemUnderTest();
		system.UseServerOptions(new BackgroundJobServerOptions
		{
			Queues = ["queue1", "queue2", "queue3"]
		});
		system.WithConfiguration(new StoredConfiguration
		{
			Containers =
			[
				new ContainerConfiguration
				{
					Tag = DefaultContainerTag.Tag(),
					Queues = ["queue1"]
				},
				new ContainerConfiguration
				{
					Tag = "secondary",
					Queues = ["queue2"]
				}
			]
		});

		var result = system.ViewModelBuilder.BuildServerConfigurations().Single();

		result.Containers[0].Queues.Should().Have.SameSequenceAs(["queue1", "queue3"]);
	}

	[Test]
	public void ShouldBuildQueuesWithAppliedLogicForNonDefaultContainer()
	{
		var system = new SystemUnderTest();
		system.UseServerOptions(new BackgroundJobServerOptions
		{
			Queues = ["queue1", "queue2", "queue3"]
		});
		system.WithConfiguration(new StoredConfiguration
		{
			Containers =
			[
				new ContainerConfiguration
				{
					Tag = DefaultContainerTag.Tag(),
					Queues = ["queue1"]
				},
				new ContainerConfiguration
				{
					Tag = "secondary",
					Queues = ["queue1", "queue4"]
				}
			]
		});

		var result = system.ViewModelBuilder.BuildServerConfigurations().Single();

		result.Containers[1].Queues.Should().Have.SameSequenceAs(["queue1"]);
	}

	[Test]
	public void ShouldBuildQueuesForDefaultContainerWithoutQueues()
	{
		var system = new SystemUnderTest();
		system.UseServerOptions(new BackgroundJobServerOptions
		{
			Queues = ["queue1", "queue2"]
		});
		system.WithConfiguration(new StoredConfiguration
		{
			Containers =
			[
				new ContainerConfiguration
				{
					Tag = DefaultContainerTag.Tag(),
					Queues = null
				}
			]
		});

		var result = system.ViewModelBuilder.BuildServerConfigurations().Single();

		result.Containers[0].Queues.Should().Have.SameSequenceAs(["queue1", "queue2"]);
	}

	[Test]
	public void ShouldBuildWithWorkerBalancerEnabledNull()
	{
		var system = new SystemUnderTest();
		system.WithConfiguration(new StoredConfiguration
		{
			Containers =
			[
				new ContainerConfiguration
				{
					WorkerBalancerEnabled = null
				}
			]
		});

		var result = system.ViewModelBuilder.BuildServerConfigurations().Single();

		result.Containers[0].WorkerBalancerEnabled.Should().Be.True();
	}

	[Test]
	public void ShouldBuildWithWorkerBalancerDisabled()
	{
		var system = new SystemUnderTest();
		system.WithConfiguration(new StoredConfiguration
		{
			Containers =
			[
				new ContainerConfiguration
				{
					WorkerBalancerEnabled = false
				}
			]
		});

		var result = system.ViewModelBuilder.BuildServerConfigurations().Single();

		result.Containers[0].WorkerBalancerEnabled.Should().Be.False();
	}

	[Test]
	public void ShouldBuildAvailableQueuesFromServerQueues()
	{
		var system = new SystemUnderTest();
		system.UseServerOptions(new BackgroundJobServerOptions
		{
			Queues = ["queue1", "queue2"]
		});
		system.WithConfiguration(new StoredConfiguration());

		var result = system.ViewModelBuilder.BuildServerConfigurations().Single();

		result.AvailableQueues.Should().Have.SameSequenceAs(["queue1", "queue2"]);
	}

	[Test]
	public void ShouldBuildAvailableQueuesDefaultToDefault()
	{
		var system = new SystemUnderTest();
		system.UseServerOptions(new BackgroundJobServerOptions());
		system.WithConfiguration(new StoredConfiguration());

		var result = system.ViewModelBuilder.BuildServerConfigurations().Single();

		result.AvailableQueues.Should().Have.SameSequenceAs(["default"]);
	}

	[Test]
	public void ShouldBuildAvailableQueuesOnlyFromServerQueues()
	{
		var system = new SystemUnderTest();
		system.UseServerOptions(new BackgroundJobServerOptions
		{
			Queues = ["queue1"]
		});
		system.WithConfiguration(new StoredConfiguration
		{
			Containers =
			[
				new ContainerConfiguration
				{
					Queues = ["queue1", "queue2"]
				},
				new ContainerConfiguration
				{
					Queues = ["queue3"]
				}
			]
		});

		var result = system.ViewModelBuilder.BuildServerConfigurations().Single();

		result.AvailableQueues.Should().Have.SameSequenceAs(["queue1"]);
	}

	[Test]
	public void ShouldUpdateContainerQueuesWithAppliedQueues()
	{
		var system = new SystemUnderTest();
		system.UseServerOptions(new BackgroundJobServerOptions
		{
			Queues = ["queue1", "queue2"]
		});
		system.WithConfiguration(new StoredConfiguration
		{
			Containers =
			[
				new ContainerConfiguration
				{
					Tag = DefaultContainerTag.Tag(),
					Queues = null
				}
			]
		});

		var result = system.ViewModelBuilder.BuildServerConfigurations().Single();

		var updated = system.Configurations().Single();
		updated.Containers[0].Queues.Should().Have.SameSequenceAs(["queue1", "queue2"]);
		result.Containers[0].Queues.Should().Have.SameSequenceAs(["queue1", "queue2"]);
	}

	[Test]
	public void ShouldUpdateContainerQueuesWithAppliedQueues2()
	{
		var system = new SystemUnderTest();
		system.UseServerOptions(new BackgroundJobServerOptions
		{
			Queues = ["queue1", "queue2", "queue3"]
		});
		system.WithConfiguration(new StoredConfiguration
		{
			Containers =
			[
				new ContainerConfiguration
				{
					Tag = DefaultContainerTag.Tag(),
					Queues = ["queue1"]
				},
				new ContainerConfiguration
				{
					Tag = "secondary",
					Queues = ["queue2"]
				}
			]
		});

		var result = system.ViewModelBuilder.BuildServerConfigurations().Single();

		var updated = system.Configurations().Single();
		updated.Containers[0].Queues.Should().Have.SameSequenceAs(["queue1", "queue3"]);
		result.Containers[0].Queues.Should().Have.SameSequenceAs(["queue1", "queue3"]);
		updated.Containers[1].Queues.Should().Have.SameSequenceAs(["queue2"]);
		result.Containers[1].Queues.Should().Have.SameSequenceAs(["queue2"]);
	}

	[Test]
	public void ShouldNotUpdateContainerQueuesWhenAlreadyCorrect()
	{
		var system = new SystemUnderTest();
		system.UseServerOptions(new BackgroundJobServerOptions
		{
			Queues = ["queue1", "queue2", "queue3"]
		});
		system.WithConfiguration(new StoredConfiguration
		{
			Name = DefaultConfigurationName.Name(),
			Active = true,
			Containers =
			[
				new ContainerConfiguration
				{
					Tag = DefaultContainerTag.Tag(),
					Queues = ["queue1", "queue3"]
				},
				new ContainerConfiguration
				{
					Tag = "secondary",
					Queues = ["queue2"]
				}
			]
		});

		system.KeyValueStore.WriteCount = 0;

		var result = system.ViewModelBuilder.BuildServerConfigurations().Single();

		var updated = system.Configurations().Single();
		// Queues should remain as they were (already correctly calculated)
		updated.Containers[0].Queues.Should().Have.SameSequenceAs(["queue1", "queue3"]);
		updated.Containers[1].Queues.Should().Have.SameSequenceAs(["queue2"]);
		result.Containers[0].Queues.Should().Have.SameSequenceAs(["queue1", "queue3"]);
		result.Containers[1].Queues.Should().Have.SameSequenceAs(["queue2"]);
		// Configuration should not have been written since queues didn't change
		system.KeyValueStore.WriteCount.Should().Be(0);
	}

	[Test]
	public void ShouldUpdateContainerQueuesWhenChanged()
	{
		var system = new SystemUnderTest();
		system.UseServerOptions(new BackgroundJobServerOptions
		{
			Queues = ["queue1", "queue2", "queue3"]
		});
		system.WithConfiguration(new StoredConfiguration
		{
			Name = DefaultConfigurationName.Name(),
			Active = true,
			Containers =
			[
				new ContainerConfiguration
				{
					Tag = DefaultContainerTag.Tag(),
					Queues = ["queue1"]
				},
				new ContainerConfiguration
				{
					Tag = "secondary",
					Queues = ["queue2"]
				}
			]
		});

		system.KeyValueStore.WriteCount = 0;

		var result = system.ViewModelBuilder.BuildServerConfigurations().Single();

		var updated = system.Configurations().Single();
		// Queues should be updated (default container should get queue3)
		updated.Containers[0].Queues.Should().Have.SameSequenceAs(["queue1", "queue3"]);
		updated.Containers[1].Queues.Should().Have.SameSequenceAs(["queue2"]);
		result.Containers[0].Queues.Should().Have.SameSequenceAs(["queue1", "queue3"]);
		result.Containers[1].Queues.Should().Have.SameSequenceAs(["queue2"]);
		// Configuration should have been written since queues changed
		system.KeyValueStore.WriteCount.Should().Be.GreaterThan(0);
	}
}