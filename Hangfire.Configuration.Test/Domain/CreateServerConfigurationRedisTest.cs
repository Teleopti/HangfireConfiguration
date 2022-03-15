﻿using System.Linq;
using NUnit.Framework;

namespace Hangfire.Configuration.Test.Domain;

public class CreateServerConfigurationRedisTest
{
	[Test]
	public void ShouldStoreConnectionString()
	{
		var system = new SystemUnderTest();

		system.ConfigurationApi.CreateServerConfiguration(new CreateRedisServerConfigurationCommand
		{
			Server = "AwesomeServer:425"
		});

		var storedConfiguration = system.ConfigurationStorage.Data.Single();
		Assert.AreEqual("AwesomeServer:425", storedConfiguration.ConnectionString);
	}

	[Test]
	public void ShouldStoreName()
	{
		var system = new SystemUnderTest();

		system.ConfigurationApi.CreateServerConfiguration(new CreateRedisServerConfigurationCommand
		{
			Name = "matte"
		});

		var storedConfiguration = system.ConfigurationStorage.Data.Single();
		Assert.AreEqual("matte", storedConfiguration.Name);
	}

	[Test]
	public void ShouldDefaultToNonActive()
	{
		var system = new SystemUnderTest();

		system.ConfigurationApi.CreateServerConfiguration(new CreateRedisServerConfigurationCommand
		{
			Server = "redis"
		});

		var storedConfiguration = system.ConfigurationStorage.Data.Single();
		Assert.False(storedConfiguration.Active);
	}

	[Test]
	public void ShouldCreateWithSchemaName()
	{
		var system = new SystemUnderTest();

		system.ConfigurationApi.CreateServerConfiguration(new CreateRedisServerConfigurationCommand
		{
			SchemaName = "my-prefix:"
		});

		var storedConfiguration = system.ConfigurationStorage.Data.Last();
		Assert.AreEqual("my-prefix:", storedConfiguration.SchemaName);
	}

	[Test]
	public void ShouldCreateWithDefaultPrefixAsSchemaName()
	{
		var system = new SystemUnderTest();

		system.ConfigurationApi.CreateServerConfiguration(new CreateRedisServerConfigurationCommand
		{
			SchemaName = null
		});

		var storedConfiguration = system.ConfigurationStorage.Data.Last();
		Assert.AreEqual(DefaultSchemaName.Redis(), storedConfiguration.SchemaName);
	}
}