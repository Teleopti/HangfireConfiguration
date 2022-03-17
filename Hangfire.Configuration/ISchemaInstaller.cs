namespace Hangfire.Configuration;

public interface ISchemaInstaller
{
	void TryConnect(string connectionString);
	void InstallHangfireConfigurationSchema(string connectionString);
	void InstallHangfireStorageSchema(string schemaName, string connectionString);
	bool HangfireStorageSchemaExists(string schemaName, string connectionString);
}