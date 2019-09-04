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
		public static void UseHangfireConfigurationUI(this IApplicationBuilder builder, string pathMatch, HangfireConfigurationUIOptions options)
#else
		public static void UseHangfireConfigurationUI(this IAppBuilder builder, string pathMatch, HangfireConfigurationUIOptions options)
#endif
		{
			builder.Map(pathMatch, subApp =>
			{
				var compositionRoot = builder.Properties.ContainsKey("CompositionRoot") ? builder.Properties["CompositionRoot"] : new CompositionRoot();
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
		HangfireConfiguration.UseHangfireConfiguration(builder, options);
	}
}