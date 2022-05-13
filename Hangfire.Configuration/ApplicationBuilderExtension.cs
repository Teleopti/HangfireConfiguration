#if NETSTANDARD2_0

using Microsoft.AspNetCore.Builder;
using Hangfire.Configuration.Web;

namespace Hangfire.Configuration
{
	public static class ApplicationBuilderExtension
	{
		public static void UseHangfireConfigurationUI(this IApplicationBuilder builder, string pathMatch, ConfigurationOptions options)
		{
			builder.Map(pathMatch, subApp =>
			{
				if (options != null)
					builder.Properties["HangfireConfigurationOptions"] = options;
				subApp.UseMiddleware<ConfigurationMiddleware>(builder.Properties);
			});
		}

		public static HangfireConfiguration UseHangfireConfiguration(this IApplicationBuilder builder, ConfigurationOptions options) =>
			new HangfireConfiguration()
				.UseApplicationBuilder(builder)
				.UseOptions(options);

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
	}
}

#endif