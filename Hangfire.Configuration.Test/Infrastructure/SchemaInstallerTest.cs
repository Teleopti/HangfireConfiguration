using System.Data.Common;
using System.Data.SqlClient;
using Dapper;
using Npgsql;
using NUnit.Framework;
using SharpTestsEx;

namespace Hangfire.Configuration.Test.Infrastructure
{
	public class SchemaInstallerTest : DatabaseTest
	{
		public SchemaInstallerTest(string connectionString) : base(connectionString)
		{
		}

		[Test]
		public void ShouldConnect()
		{
			var creator = new SchemaInstaller();

			creator.TryConnect(ConnectionString);
		}

		[Test]
		public void ShouldThrowExceptionWhenNoDatabase()
		{
			var creator = new SchemaInstaller();
			var connectionString = SelectDialect(
				() => new SqlConnectionStringBuilder(ConnectionString) {InitialCatalog = "Does_Not_Exist"}.ToString(),
				() => new NpgsqlConnectionStringBuilder(ConnectionString) {Database = "Does_Not_Exist"}.ToString());
			
			var exception = Assert.Catch(() => creator.TryConnect(connectionString));
			
			exception.Message.Should().Contain("Does_Not_Exist");
		}

		[Test]
		public void ShouldCreateSchema()
		{
			var creator = new SchemaInstaller();

			creator.InstallHangfireStorageSchema("hangfiretestschema", ConnectionString);

			using var conn = SelectDialect<DbConnection>(() => new SqlConnection(ConnectionString), () => new NpgsqlConnection(ConnectionString));
			Assert.AreEqual("hangfiretestschema", conn.ExecuteScalar<string>("SELECT SCHEMA_NAME FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = 'hangfiretestschema'"));
		}

		[Test]
		public void ShouldCreateSchemaWithDefaultSchemaName()
		{
			var creator = new SchemaInstaller();

			creator.InstallHangfireStorageSchema("", ConnectionString);

			var expected = SelectDialect(() => "HangFire", () => "hangfire");
			using var conn = SelectDialect<DbConnection>(() => new SqlConnection(ConnectionString), () => new NpgsqlConnection(ConnectionString));
			Assert.AreEqual(expected, conn.ExecuteScalar<string>($"SELECT SCHEMA_NAME FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = '{expected}'"));
		}

		[Test]
		public void ShouldIndicateThatSchemaExists()
		{
			var creator = new SchemaInstaller();
			creator.InstallHangfireStorageSchema("schema", ConnectionString);

			var result = creator.HangfireStorageSchemaExists("schema", ConnectionString);

			Assert.True(result);
		}

		[Test]
		public void ShouldIndicateThatSchemaDoesNotExists()
		{
			var creator = new SchemaInstaller();

			var result = creator.HangfireStorageSchemaExists("nonExistingSchema", ConnectionString);

			Assert.False(result);
		}

		[Test]
		public void ShouldInstallConfigurationSchema()
		{
			var creator = new SchemaInstaller();

			creator.InstallHangfireConfigurationSchema(ConnectionString);

			var expected = "hangfireconfiguration";
			using var conn = SelectDialect<DbConnection>(() => new SqlConnection(ConnectionString), () => new NpgsqlConnection(ConnectionString));
			var actual = conn.ExecuteScalar<string>($"SELECT SCHEMA_NAME FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = '{expected}'");
			Assert.AreEqual(expected.ToLower(), actual.ToLower());
		}
	}
}