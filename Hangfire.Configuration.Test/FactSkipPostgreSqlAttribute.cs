using Xunit;

namespace Hangfire.Configuration.Test
{
	public class FactSkipPostgreSqlAttribute : FactAttribute
	{
		public FactSkipPostgreSqlAttribute()
		{
			if(new ConnectionStringDialectSelector(ConnectionUtils.GetConnectionString()).IsPostgreSql())
				Skip = "skip test for postgresql";
		}
	}
}