#if NETSTANDARD2_0

using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Hangfire.Dashboard;

namespace Hangfire.Configuration.Web;

public class DynamicHangfireDashboardsMiddleware
{
	private readonly RequestDelegate _next;
	private readonly HangfireConfiguration _configuration;
	private readonly DashboardOptions _dashboardOptions;

	public DynamicHangfireDashboardsMiddleware(
			RequestDelegate next,
			IDictionary<string, object> properties)
	{
		_next = next;

		var injected = properties?.ContainsKey("HangfireConfiguration") ?? false;
		if (injected)
		{
			_configuration = (HangfireConfiguration) properties["HangfireConfiguration"];
		}
		else
		{
			_configuration = new HangfireConfiguration();
			var options = (ConfigurationOptions) properties["HangfireConfigurationOptions"];
			_configuration.UseOptions(options);
		}

		_dashboardOptions = (DashboardOptions) properties["HangfireDashboardOptions"];
	}

	public async Task Invoke(HttpContext context)
	{
		var pathRegex = new Regex($@"\/([0-7]+)(.*)($|\/)");
		var matches = pathRegex.Match(context.Request.Path.Value);
		var configurationId = int.Parse(matches.Groups[1].Value);
		var resource = matches.Groups[2].Value;

		var configuration = _configuration
			.QueryAllBackgroundJobServers()
			.Single(x => x.ConfigurationId == configurationId);

		var originalPath = context.Request.Path;
		var originalPathBase = context.Request.PathBase;

		context.Request.PathBase += "/" + configurationId;
		context.Request.Path = resource;

		try
		{
			await new AspNetCoreDashboardMiddleware(
				_next,
				configuration.JobStorage,
				_dashboardOptions,
				DashboardRoutes.Routes
			).Invoke(context);
		}
		finally
		{
			context.Request.PathBase = originalPathBase;
			context.Request.Path = originalPath;
		}
	}
}

#endif