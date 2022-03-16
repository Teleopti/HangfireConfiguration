using NUnit.Framework;

namespace Hangfire.Configuration.Test.Integration;

[Parallelizable(ParallelScope.None)]
public class DoStuffTest
{
	[Test]
	public void DoTheFoo()
	{
		DatabaseTestSetup.SetupSqlServer(ConnectionStrings.SqlServer);
		
		
	}
}