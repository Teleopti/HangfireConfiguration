using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using Npgsql;
using NUnit.Framework;

namespace Hangfire.Configuration.Test.Infrastructure
{
	public class HangfireConfigurationSchemaInstallerTest : DatabaseTest
	{
		public HangfireConfigurationSchemaInstallerTest(string connectionString) : base(connectionString)
		{
		}

		[Test]
		public void ShouldUpgradeFrom0ToLatest()
		{
			using var c = SelectDialect<DbConnection>(() => new SqlConnection(ConnectionString), () => new NpgsqlConnection(ConnectionString));
			HangfireConfigurationSchemaInstaller.Install(c);

			Assert.AreEqual(HangfireConfigurationSchemaInstaller.SchemaVersion, version());
		}

		private int version()
		{
			var schemaName = SelectDialect(() => "[Schema]", () => "schema");
			using var c = SelectDialect<DbConnection>(() => new SqlConnection(ConnectionString), () => new NpgsqlConnection(ConnectionString));
			return c.Query<int>($"SELECT Version FROM HangfireConfiguration.{schemaName}").Single();
		}
	}
}