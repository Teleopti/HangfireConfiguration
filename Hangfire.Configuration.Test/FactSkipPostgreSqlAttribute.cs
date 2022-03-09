using System;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace Hangfire.Configuration.Test
{
	public class FactSkipPostgreSqlAttribute : Attribute, ITestAction
	{
		public ActionTargets Targets => ActionTargets.Test;

		public void BeforeTest(ITest test)
		{
			if (new ConnectionStringDialectSelector(ConnectionUtils.GetConnectionString()).IsPostgreSql())
				Assert.Ignore("skip test for postgresql");
		}

		public void AfterTest(ITest test)
		{
		}
	}
}