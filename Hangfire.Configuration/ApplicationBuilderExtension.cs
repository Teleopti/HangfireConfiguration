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
				if (options != null)
					builder.Properties["HangfireConfigurationOptions"] = options;
#if NETSTANDARD2_0
				subApp.UseMiddleware<ConfigurationMiddleware>(builder.Properties);
#else
				subApp.Use(typeof(ConfigurationMiddleware), builder.Properties);
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

#if NETSTANDARD2_0
		public static void UseDynamicHangfireDashboards(
			this IApplicationBuilder builder,
			string pathMatch,
			ConfigurationOptions options,
			DashboardOptions dashboardOptions)
		{
			builder.Map(pathMatch, subApp =>
			{
				subApp.UseMiddleware<DynamicHangfireDashboardsMiddleware>(
					options,
					dashboardOptions
				);
			});
		}
#endif
		
	}
}