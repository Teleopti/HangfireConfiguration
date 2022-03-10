using System.Linq;
using Dapper;
using Hangfire.Configuration.Internals;
using NUnit.Framework;

namespace Hangfire.Configuration.Test.Infrastructure
{
	public class SqlServerObjectsInstallerTest : DatabaseTestBase
	{
		public SqlServerObjectsInstallerTest(string connectionString) : base(connectionString)
		{
		}

		[Test]
		public void ShouldUpgradeFrom0ToLatest()
		{
			DatabaseTestSetup.Setup(ConnectionString);

			using var c = ConnectionString.CreateConnection();
			SqlServerObjectsInstaller.Install(c);

			Assert.AreEqual(SqlServerObjectsInstaller.SchemaVersion, version());
		}

		private int version()
		{
			var schemaName = new ConnectionStringDialectSelector(ConnectionString)
				.SelectDialect(() => "[Schema]", () => "schema");
			using var c = ConnectionString.CreateConnection();
			return c.Query<int>($"SELECT Version FROM HangfireConfiguration.{schemaName}").Single();
		}
	}
}