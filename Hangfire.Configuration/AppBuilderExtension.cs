using System;
using Owin;

namespace Hangfire.Configuration
{
	public static class AppBuilderExtension
	{
		public static void UseHangfireConfigurationInterface(this IAppBuilder builder, string pathMatch, HangfireConfigurationInterfaceOptions options)
		{
			builder.Map(pathMatch, subApp =>
			{
				var compositionRoot = builder.Properties.ContainsKey("CompositionRoot") ? builder.Properties["CompositionRoot"] : null;
				subApp.Use(typeof(ConfigurationMiddleware), options, compositionRoot);
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
		public int DefaultGoalWorkerCount { get; set; } = 10;
		public int MinimumWorkerCount { get; set; } = 1;
		public int MaximumGoalWorkerCount { get; set; } = 100;
		public int MinimumServers { get; set; } = 2;
	}
	
	public class HangfireConfigurationInterfaceOptions
	{
		public string ConnectionString;
		public bool PrepareSchemaIfNecessary;
		public bool AllowNewServerCreation;
	}
}