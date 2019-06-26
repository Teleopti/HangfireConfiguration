using System.Net;
using System.Threading.Tasks;
using Hangfire.Configuration.Pages;
using Microsoft.Owin;

namespace Hangfire.Configuration
{
	public class ConfigurationMiddleware : OwinMiddleware
	{
		private readonly Configuration _configuration;

		public ConfigurationMiddleware(OwinMiddleware next, string connectionString) : base(next)
		{
			_configuration = new Configuration(
				new ConfigurationRepository(connectionString)
			);
		}

		public override async Task Invoke(IOwinContext context)
		{
			var page = new ConfigurationPage(_configuration, context.Request.PathBase.Value);

			if (context.Request.Path.StartsWithSegments(new PathString("/saveConfig")))
			{
				var workers = tryParseNullable(context.Request.Query["workers"]);
				_configuration.WriteGoalWorkerCount(workers);
				page.DisplayConfirmationMessage();
			}

			var html = page.ToString();
			context.Response.StatusCode = (int) HttpStatusCode.OK;
			context.Response.ContentType = "text/html";
			await context.Response.WriteAsync(html);
		}

		private int? tryParseNullable(string value) => 
			int.TryParse(value, out var outValue) ? (int?) outValue : null;
	}
}