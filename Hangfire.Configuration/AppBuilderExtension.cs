#if NETSTANDARD2_0
using Microsoft.AspNetCore.Builder;
#else
using Owin;
#endif

namespace Hangfire.Configuration
{
	public static class ApplicationBuilderExtension
	{
#if NETSTANDARD2_0
		public static void UseHangfireConfigurationInterface(this IApplicationBuilder builder, string pathMatch, HangfireConfigurationInterfaceOptions options)
#else
		public static void UseHangfireConfigurationInterface(this IAppBuilder builder, string pathMatch, HangfireConfigurationInterfaceOptions options)
#endif
		{
			builder.Map(pathMatch, subApp =>
			{
				var compositionRoot = builder.Properties.ContainsKey("CompositionRoot") ? builder.Properties["CompositionRoot"] : null;
#if NETSTANDARD2_0
				subApp.UseMiddleware<ConfigurationMiddleware>(options, compositionRoot);
#else
				subApp.Use(typeof(ConfigurationMiddleware), options, compositionRoot);
#endif
			});
		}

#if NETSTANDARD2_0
		public static HangfireConfiguration UseHangfireConfiguration(this IApplicationBuilder builder, ConfigurationOptions options) =>
#else
		public static HangfireConfiguration UseHangfireConfiguration(this IAppBuilder builder, ConfigurationOptions options) =>
#endif
		new HangfireConfiguration(null, options);
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