namespace Hangfire.Configuration.Test
{
	public static class ConnectionUtils
	{
		public const string DefaultConnectionStringTemplate
			= @"Data Source=.;Integrated Security=True;Initial Catalog=Hangfire.Configuration.Tests;";

		public static string GetConnectionString() => DefaultConnectionStringTemplate;

		public static string GetLoginUser() => "HangfireTest";
		public static string GetLoginUserPassword() => "test";
	}
}