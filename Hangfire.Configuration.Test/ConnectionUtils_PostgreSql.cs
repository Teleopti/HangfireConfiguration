namespace Hangfire.Configuration.Test
{
	public static class ConnectionUtils
	{
		// pooling needs to be off for some reason?
		public const string DefaultConnectionStringTemplate
			= @"User ID=postgres;Password=root;Host=localhost;Database=Hangfire.Configuration.Tests;Pooling=false;";
		
		public static string GetConnectionString() => DefaultConnectionStringTemplate;
	}
}