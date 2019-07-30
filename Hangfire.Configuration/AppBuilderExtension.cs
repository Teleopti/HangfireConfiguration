using Hangfire.SqlServer;
using Owin;

namespace Hangfire.Configuration
{
	public static class AppBuilderExtension
	{
		public static void UseHangfireConfigurationInterface(this IAppBuilder builder, string pathMatch, HangfireConfigurationInterfaceOptions options)
		{
			builder.Map(pathMatch, subApp =>
			{
				subApp.Use(typeof(ConfigurationMiddleware), options);
			});
		}
		
		public static HangfireConfiguration UseHangfireConfiguration(this IAppBuilder builder, ConfigurationOptions options) =>
			new HangfireConfiguration(builder, options);
	}
	
	public class ConfigurationOptions
	{
		public string ConnectionString { get; set; }
		public string DefaultHangfireConnectionString { get; set; }
		public string DefaultSchemaName { get; set; }
		public int? DefaultGoalWorkerCount { get; set; }
		public int? MinimumWorkerCount { get; set; }
		public int? MaximumGoalWorkerCount { get; set; }
		public int? MinimumServers { get; set; }
	}
	
	public class HangfireConfigurationInterfaceOptions
	{
		public string ConnectionString;
		public bool PrepareSchemaIfNecessary;
		public bool AllowNewServerCreation;
	}
}