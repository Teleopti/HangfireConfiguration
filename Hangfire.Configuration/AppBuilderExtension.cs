using System;
using Microsoft.AspNetCore.Builder;
using Hangfire.SqlServer;

namespace Hangfire.Configuration
{
	public static class AppBuilderExtension
	{
		public static void UseHangfireConfigurationInterface(this IApplicationBuilder builder, string pathMatch, HangfireConfigurationInterfaceOptions options)
        {
			builder.Map(pathMatch, subApp =>
			{
				if (builder.Properties.TryGetValue("CompositionRoot", out var compositionRoot))
                    subApp.UseMiddleware<ConfigurationMiddleware>(options, compositionRoot as CompositionRoot);
                else
                    subApp.UseMiddleware<ConfigurationMiddleware>(options);
            });
		}
		
		public static HangfireConfiguration UseHangfireConfiguration(this IApplicationBuilder builder, ConfigurationOptions options) =>
			new HangfireConfiguration(builder, options);
	}
	
	public class ConfigurationOptions
	{
		public string ConnectionString { get; set; }
		public string DefaultHangfireConnectionString { get; set; }
		public string DefaultSchemaName { get; set; }
		public int DefaultGoalWorkerCount { get; set; } = 10;
		public int MinimumWorkerCount { get; set; } = 1;
		public int MaximumGoalWorkerCount { get; set; } = 100;
		public int MinimumServers { get; set; } = 2;
		public SqlServerStorageOptions StorageOptions { get; set; }
	}
	
	public class HangfireConfigurationInterfaceOptions
	{
		public string ConnectionString;
		public bool PrepareSchemaIfNecessary;
		public bool AllowNewServerCreation;
	}
}