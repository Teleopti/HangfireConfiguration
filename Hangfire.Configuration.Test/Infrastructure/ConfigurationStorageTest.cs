using System.Linq;
using NUnit.Framework;

namespace Hangfire.Configuration.Test.Infrastructure
{
    public class ConfigurationStorageTest : DatabaseTestBase
    {
	    public ConfigurationStorageTest(string connectionString) : base(connectionString)
	    {
	    }

        [Test]
        public void ShouldReadEmptyConfiguration()
        {
            var storage = new ConfigurationStorage(ConnectionString);

            Assert.IsEmpty(storage.ReadConfigurations());
        }

        [Test]
        public void ShouldWrite()
        {
            var storage = new ConfigurationStorage(ConnectionString);

            storage.WriteConfiguration(new StoredConfiguration
            {
                ConnectionString = "connection string",
                SchemaName = "schema name",
                Active = false
            });

            var configuration = storage.ReadConfigurations().Single();
            Assert.AreEqual("connection string", configuration.ConnectionString);
            Assert.AreEqual("schema name", configuration.SchemaName);
            Assert.AreEqual(false, configuration.Active);
        }

        [Test]
        public void ShouldRead()
        {
            var storage = new ConfigurationStorage(ConnectionString);
            storage.WriteConfiguration(new StoredConfiguration
            {
                ConnectionString = "connectionString",
                SchemaName = "schemaName",
                GoalWorkerCount = 3,
                Active = true
            });

            var result = storage.ReadConfigurations().Single();

            Assert.AreEqual(1, result.Id);
            Assert.AreEqual("connectionString", result.ConnectionString);
            Assert.AreEqual("schemaName", result.SchemaName);
            Assert.AreEqual(3, result.GoalWorkerCount);
            Assert.AreEqual(true, result.Active);
        }

        [Test]
        public void ShouldUpdate()
        {
            var storage = new ConfigurationStorage(ConnectionString);
            storage.WriteConfiguration(new StoredConfiguration());

            var existing = storage.ReadConfigurations().Single();
            existing.ConnectionString = "connection";
            existing.SchemaName = "schema";
            existing.GoalWorkerCount = 23;
            existing.Active = true;
            storage.WriteConfiguration(existing);

            var configuration = storage.ReadConfigurations().Single();
            Assert.AreEqual("connection", configuration.ConnectionString);
            Assert.AreEqual("schema", configuration.SchemaName);
            Assert.AreEqual(23, configuration.GoalWorkerCount);
            Assert.AreEqual(true, configuration.Active);
        }

        [Test]
        public void ShouldWriteName()
        {
            var storage = new ConfigurationStorage(ConnectionString);

            storage.WriteConfiguration(new StoredConfiguration
            {
                Name = "name",
            });

            var configuration = storage.ReadConfigurations().Single();
            Assert.AreEqual("name", configuration.Name);
        }

        [Test]
        public void ShouldUpdateName()
        {
            var storage = new ConfigurationStorage(ConnectionString);
            storage.WriteConfiguration(new StoredConfiguration());

            var existing = storage.ReadConfigurations().Single();
            existing.Name = "name";
            storage.WriteConfiguration(existing);

            var configuration = storage.ReadConfigurations().Single();
            Assert.AreEqual("name", configuration.Name);
        }
    }
}