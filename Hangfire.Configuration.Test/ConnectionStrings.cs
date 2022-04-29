namespace Hangfire.Configuration.Test
{
	public static class ConnectionStrings
	{
		public const string SqlServer = @"Data Source=.;Initial Catalog=Hangfire.Configuration.Tests;Integrated Security=True;";
		public const string Postgres = @"Host=localhost;Database=Hangfire.Configuration.Tests;Username=postgres;Password=Password12!;Pooling=false;";
	}
}