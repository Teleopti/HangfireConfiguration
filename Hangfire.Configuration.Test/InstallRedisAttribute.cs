using System;
using System.Diagnostics;
using System.IO;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace Hangfire.Configuration.Test;

public class InstallRedisAttribute : Attribute, ITestAction
{
	private Process redis;
	
	public void BeforeTest(ITest test)
	{
		redis = Process.Start(Path.Combine(TestContext.CurrentContext.TestDirectory, "redis-server.exe"));
	}

	public void AfterTest(ITest test)
	{
		redis.Kill();
	}

	public ActionTargets Targets { get; }
}