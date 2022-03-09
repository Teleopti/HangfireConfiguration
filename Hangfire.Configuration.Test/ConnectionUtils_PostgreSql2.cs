namespace Hangfire.Configuration.Test
{
	public static class ConnectionUtilsPostgres
	{
		public const string DefaultConnectionStringTemplate
			= @"User ID=postgres;Password=root;Host=localhost;Database=Hangfire.Configuration.Tests;Pooling=false;";
		
		public static string GetConnectionString() => DefaultConnectionStringTemplate;
	}
}