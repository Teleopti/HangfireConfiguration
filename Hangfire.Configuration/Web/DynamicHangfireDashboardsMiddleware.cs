#if NETSTANDARD2_0

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
		ConfigurationOptions options,
		DashboardOptions dashboardOptions)
	{
		_next = next;
		_dashboardOptions = dashboardOptions;
		_configuration = new HangfireConfiguration();
		_configuration.UseOptions(options);
	}

	public async Task Invoke(HttpContext context)
	{
		var pathRegex = new Regex($@"\/([0-7]+)(.*)($|\/)");
		var matches = pathRegex.Match(context.Request.Path.Value);
		var configurationId = int.Parse(matches.Groups[1].Value);
		var resource = matches.Groups[2].Value;

		var configurations = _configuration
			.QueryAllWorkerServers()
			.OrderByDescending(x => x.Publisher)
			.ThenByDescending(x => x.ConfigurationId)
			.ToArray();
		var configuration = configurations.Single(x => x.ConfigurationId == configurationId);

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