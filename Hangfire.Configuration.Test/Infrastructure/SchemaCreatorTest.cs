using System;
using Dapper;
using Hangfire.Configuration.Internals;
using NUnit.Framework;
using SharpTestsEx;

namespace Hangfire.Configuration.Test.Infrastructure
{
	public class SchemaCreatorTest : DatabaseTestBase
	{
		public SchemaCreatorTest(string connectionString) : base(connectionString)
		{
		}

		[Test]
		public void ShouldConnect()
		{
			var creator = new HangfireSchemaCreator();

			creator.TryConnect(ConnectionString);
		}

		[Test]
		public void ShouldThrowExceptionWhenNoDatabase()
		{
			var creator = new HangfireSchemaCreator();

			var connectionString = ConnectionString.ChangeDatabase("Does_Not_Exist");

			var exception = Assert.Catch(() => creator.TryConnect(connectionString));
			exception.Message.Should().Contain("Does_Not_Exist");
		}

		[Test]
		public void ShouldCreateSchema()
		{
			var creator = new HangfireSchemaCreator();

			creator.CreateHangfireStorageSchema("hangfiretestschema", ConnectionString);

			using var conn = ConnectionString.CreateConnection();
			Assert.AreEqual("hangfiretestschema", conn.ExecuteScalar<string>("SELECT SCHEMA_NAME FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = 'hangfiretestschema'"));
		}

		[Test]
		public void ShouldCreateSchemaWithDefaultSchema()
		{
			var creator = new HangfireSchemaCreator();

			creator.CreateHangfireStorageSchema("", ConnectionString);

			var expected = DefaultSchemaName;
			using var conn = ConnectionString.CreateConnection();
			Assert.AreEqual(expected, conn.ExecuteScalar<string>($"SELECT SCHEMA_NAME FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = '{expected}'"));
		}

		[Test]
		public void ShouldThrowOnCreateWhenInvalidConnectionString()
		{
			var creator = new HangfireSchemaCreator();

			Assert.Throws<Exception>(() => creator.CreateHangfireStorageSchema("HangfireTestSchema", "InvalidConnectionString"));
		}

		[Test]
		public void ShouldIndicateThatSchemaExists()
		{
			var creator = new HangfireSchemaCreator();
			creator.CreateHangfireStorageSchema("schema", ConnectionString);

			var result = creator.HangfireStorageSchemaExists("schema", ConnectionString);

			Assert.True(result);
		}

		[Test]
		public void ShouldIndicateThatSchemaDoesNotExists()
		{
			var creator = new HangfireSchemaCreator();

			var result = creator.HangfireStorageSchemaExists("nonExistingSchema", ConnectionString);

			Assert.False(result);
		}
	}
}