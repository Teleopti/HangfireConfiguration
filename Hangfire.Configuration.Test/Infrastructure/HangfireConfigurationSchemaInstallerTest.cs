using System.Linq;
using Dapper;
using DbAgnostic;
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
			using var c = ConnectionString.CreateConnection();
			HangfireConfigurationSchemaInstaller.Install(c);

			Assert.AreEqual(HangfireConfigurationSchemaInstaller.SchemaVersion, version());
		}

		private int version()
		{
			var schemaName = ConnectionString.PickDialect("[Schema]", "schema");
			using var c = ConnectionString.CreateConnection();
			return c.Query<int>($"SELECT Version FROM HangfireConfiguration.{schemaName}").Single();
		}
	}
}