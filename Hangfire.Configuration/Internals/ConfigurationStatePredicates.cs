namespace Hangfire.Configuration.Internals;

internal static class ConfigurationStatePredicates
{
	public static bool Matches(this ConfigurationState state, string connectionString, string schemaName)
	{
		if (state.ConnectionString == null)
			return false;
		if (connectionString == null)
			return false;
		if (state.SchemaName == null)
			return false;
		if (schemaName == null)
			return false;
		return state.ConnectionString == connectionString &&
		       state.SchemaName == schemaName;
	}

	public static bool Matches(this ConfigurationState state, StoredConfiguration stored)
	{
		if (state.Configuration?.Id == stored.Id)
			return true;
		return state.Matches(stored.ConnectionString, stored.SchemaName);
	}
}