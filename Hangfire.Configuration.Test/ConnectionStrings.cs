namespace Hangfire.Configuration.Test
{
	public static class ConnectionStrings
	{
		// public const string SqlServer = @"Data Source=.;Initial Catalog=Hangfire.Configuration.Tests;Integrated Security=True;";
		public const string SqlServer = @"Data Source=localhost;Initial Catalog=Hangfire.Configuration.Tests;User Id=sa;Password=P@ssw0rd;";
		public const string Postgres = @"Host=localhost;Database=Hangfire.Configuration.Tests;Username=postgres;Password=root;Pooling=false;";
	}
}