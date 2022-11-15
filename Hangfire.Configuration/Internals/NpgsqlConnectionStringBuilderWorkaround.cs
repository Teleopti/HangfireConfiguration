using System;
using Npgsql;

namespace Hangfire.Configuration.Internals;

// Setting IntegratedSecurity property may throw if the environment this runs on is linux
internal static class NpgsqlConnectionStringBuilderWorkaround
{
	public static string SetIntegratedSecurity(string connectionString)
	{
		return connectionString + ";Integrated Security=True";
	}

	public static NpgsqlConnectionStringBuilder Parse(string connectionString)
	{
		try
		{
			return new NpgsqlConnectionStringBuilder(connectionString);
		}
		//System.ArgumentException : Format of the initialization string does not conform to specification starting at index 0.
		//System.ArgumentException : Couldn't set integrated security
		catch (ArgumentException e)
		{
			if (e.Message.Contains("Couldn't set integrated security"))
			{
				connectionString = connectionString.Replace("Integrated Security=true", "");
				return new NpgsqlConnectionStringBuilder(connectionString);
			}
			return new NpgsqlConnectionStringBuilder(connectionString);
		}
	}
}