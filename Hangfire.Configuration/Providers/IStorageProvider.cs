namespace Hangfire.Configuration.Providers;

public interface IStorageProvider
{
	object CopyOptions(object options);
	object NewOptions();
	bool OptionsIsSuitable(object options);
	
	string DefaultSchemaName();
	void AssignSchemaName(object options, string schemaName);
	
	JobStorage NewStorage(string connectionString, object options);
}