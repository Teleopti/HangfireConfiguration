using System;
using System.Diagnostics;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;

namespace Hangfire.Configuration.Test;

public class InstallRedisAttribute : Attribute, ITestAction
{
	private Process redis;
	
	public void BeforeTest(ITest test)
	{
		var parallelScope = TestContext.CurrentContext.Test.Properties.Get(PropertyNames.ParallelScope);
		if (parallelScope == null || (ParallelScope)parallelScope != ParallelScope.None)
			throw new Exception("You cannot run redis tests in parallell with other test. Add [Parallelizable(ParallelScope.None)]!");
		redis = Process.Start($"{Environment.GetEnvironmentVariable("USERPROFILE")}/.nuget/packages/redis-64/3.0.503/tools/redis-server.exe");
	}

	public void AfterTest(ITest test)
	{
		redis?.Kill();
	}

	public ActionTargets Targets { get; }
}