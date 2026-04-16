#if NETSTANDARD2_0

using Microsoft.AspNetCore.Builder;
using Hangfire.Configuration.Web;

namespace Hangfire.Configuration;

public static class ApplicationBuilderExtension
{
	public static void UseHangfireConfigurationUI(
		this IApplicationBuilder builder, 
		string pathMatch)
	{
		builder.Map(pathMatch, subApp =>
		{
			subApp.UseMiddleware<ConfigurationMiddleware>(builder.Properties);
		});
	}

	public static HangfireConfiguration UseHangfireConfiguration(this IApplicationBuilder builder, ConfigurationOptions options)
	{
		var configuration = new HangfireConfiguration()
			.UseApplicationBuilder(builder)
			.UseOptions(options);
		builder.Properties["HangfireConfiguration"] = configuration;
		return configuration;
	}

	public static void UseHangfireConfiguration(this IApplicationBuilder builder, HangfireConfiguration configuration)
	{
		configuration.UseApplicationBuilder(builder);
		builder.Properties["HangfireConfiguration"] = configuration;
	}

	public static void UseDynamicHangfireDashboards(
		this IApplicationBuilder builder,
		string pathMatch,
		DashboardOptions dashboardOptions)
	{
		builder.Map(pathMatch, subApp =>
		{
			builder.Properties["HangfireDashboardOptions"] = dashboardOptions;
			subApp.UseMiddleware<DynamicHangfireDashboardsMiddleware>(builder.Properties);
		});
	}
}

#endif