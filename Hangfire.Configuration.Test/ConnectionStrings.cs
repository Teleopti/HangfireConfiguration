namespace Hangfire.Configuration.Test
{
	public static class ConnectionStrings
	{
		public const string SqlServer = @"Data Source=.;Integrated Security=True;Initial Catalog=Hangfire.Configuration.Tests;";
		public const string Postgres = @"User ID=postgres;Password=root;Host=localhost;Database=Hangfire.Configuration.Tests;Pooling=false;";
	}
}