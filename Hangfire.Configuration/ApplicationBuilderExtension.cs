#if NETSTANDARD2_0
using Microsoft.AspNetCore.Builder;
#else
using Owin;
#endif
using Hangfire.Configuration.Web;

namespace Hangfire.Configuration
{
	public static class ApplicationBuilderExtension
	{
#if NETSTANDARD2_0
		public static void UseHangfireConfigurationUI(this IApplicationBuilder builder, string pathMatch, ConfigurationOptions options)
#else
		public static void UseHangfireConfigurationUI(this IAppBuilder builder, string pathMatch, ConfigurationOptions options)
#endif
		{
			builder.Map(pathMatch, subApp =>
			{
#if NETSTANDARD2_0
				subApp.UseMiddleware<ConfigurationMiddleware>(options, builder.Properties);
#else
				subApp.Use(typeof(ConfigurationMiddleware), options, builder.Properties);
#endif
			});
		}

#if NETSTANDARD2_0
		public static HangfireConfiguration UseHangfireConfiguration(this IApplicationBuilder builder, ConfigurationOptions options) =>
#else
		public static HangfireConfiguration UseHangfireConfiguration(this IAppBuilder builder, ConfigurationOptions options) =>
#endif
			new HangfireConfiguration()
				.UseApplicationBuilder(builder)
				.UseOptions(options);
	}
}