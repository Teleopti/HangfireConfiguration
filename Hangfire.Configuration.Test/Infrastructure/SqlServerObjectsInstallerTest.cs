using System.Linq;
using Dapper;
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
			using var c = ConnectionString.CreateConnection();
			SqlServerObjectsInstaller.Install(c);

			Assert.AreEqual(SqlServerObjectsInstaller.SchemaVersion, version());
		}

		private int version()
		{
			var schemaName = SelectDialect(() => "[Schema]", () => "schema");
			using var c = ConnectionString.CreateConnection();
			return c.Query<int>($"SELECT Version FROM HangfireConfiguration.{schemaName}").Single();
		}
	}
}