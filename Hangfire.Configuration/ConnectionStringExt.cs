using System;

namespace Hangfire.Configuration;

public static class ConnectionStringExt
{
	private const string redisStart = "redis$$";
	
	public static void Run(this string connString, Action<string> redis, Action relationalDb)
	{
		Select(connString, s =>
			{
				redis(s);
				return 0;
			},
			() =>
			{
				relationalDb();
				return 0;
			});
	}
	
	public static T Select<T>(this string connString, Func<string, T> redis, Func<T> relationalDb)
	{
		return connString != null && connString.StartsWith(redisStart) ? 
			redis(connString.Substring(redisStart.Length)) : relationalDb();
	}
}