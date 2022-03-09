namespace Hangfire.Configuration;

public interface IHangfireSchemaCreator
{
	void TryConnect(string connectionString);
	void CreateHangfireStorageSchema(string schemaName, string connectionString);
	bool HangfireStorageSchemaExists(string schemaName, string connectionString);
}