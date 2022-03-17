using System;
using System.Diagnostics;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace Hangfire.Configuration.Test;

public class InstallRedisAttribute : Attribute, ITestAction
{
	private Process redis;
	
	public void BeforeTest(ITest test)
	{
		redis = Process.Start($"{Environment.GetEnvironmentVariable("USERPROFILE")}/.nuget/packages/redis-64/3.0.503/tools/redis-server.exe");
	}

	public void AfterTest(ITest test)
	{
		redis.Kill();
	}

	public ActionTargets Targets { get; }
}