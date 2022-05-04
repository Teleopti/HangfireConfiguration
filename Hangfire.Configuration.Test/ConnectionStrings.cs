namespace Hangfire.Configuration.Test
{
	public static class ConnectionStrings
	{
#if DEBUG
		public const string SqlServer = @"Data Source=.;Initial Catalog=Hangfire.Configuration.Tests;Integrated Security=True;";
#else
		public const string SqlServer = @"Data Source=localhost;Initial Catalog=Hangfire.Configuration.Tests;User Id=sa;Password=P@ssw0rd2";
#endif
		
		public const string Postgres = @"Host=localhost;Database=Hangfire.Configuration.Tests;Username=postgres;Password=root;Pooling=false;";
	}
}