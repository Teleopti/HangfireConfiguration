﻿using System.Linq;
using Xunit;

namespace Hangfire.Configuration.Test.Domain
{
    public class ActivateStorageConfigurationTest
    {
        [Fact]
        public void ShouldBeInactiveWhenCreated()
        {
            var system = new SystemUnderTest();
            
            system.Configuration.CreateStorageConfiguration(new NewStorageConfiguration()
            {
                Server = "AwesomeServer",
                Database = "TestDatabase",
                User = "testUser",
                Password = "awesomePassword",
                SchemaName = "awesomeSchema"
            });

            var storedConfiguration = system.Repository.Data.Single();
            Assert.Equal(false, storedConfiguration.Active);
        }

        [Fact]
        public void ShouldActivate()
        {
            var system = new SystemUnderTest();
            system.Repository.Has(new StoredConfiguration
            {
                Id = 1,
                ConnectionString = "connectionString",
                SchemaName = "awesomeSchema",
                Active = false
            });

            system.Configuration.ActivateStorage(1);

            var storedConfiguration = system.Repository.Data.Single();
            Assert.Equal(true, storedConfiguration.Active);
        }

        [Fact]
        public void ShouldDeactivatePreviouslyActive()
        {
            var system = new SystemUnderTest();
            system.Repository.Has(
                new StoredConfiguration {Id = 1, Active = true, ConnectionString = "connectionString", SchemaName = "awesomeSchema"},
                new StoredConfiguration {Id = 2, ConnectionString = "connectionString2", SchemaName = "awesomeSchema2"}
            );

            system.Configuration.ActivateStorage(2);

            Assert.Equal(false, system.Repository.Data.Single(x => x.Id == 1).Active);
        }
    }
}