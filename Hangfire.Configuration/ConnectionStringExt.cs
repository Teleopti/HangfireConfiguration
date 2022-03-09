namespace Hangfire.Configuration;

public static class ConnectionStringExt
{
	public const string RedisStart = "redis$$";
	
	public static string TrimRedisPrefix(this string connString)
	{
		return connString != null && connString.StartsWith(RedisStart) ? 
			connString.Substring(RedisStart.Length) : 
			connString;
	}
}