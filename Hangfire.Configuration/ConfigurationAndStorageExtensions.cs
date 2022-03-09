namespace Hangfire.Configuration;

internal static class ConfigurationAndStorageExtensions
{
	internal static ConfigurationInfo ToConfigurationInfo(this ConfigurationAndStorage instance) =>
		new()
		{
			ConfigurationId = instance.Configuration.Id.Value,
			Name = instance.Configuration.Name,
			JobStorage = instance.CreateJobStorage()
		};
}