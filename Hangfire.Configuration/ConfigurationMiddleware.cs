using System;
using System.Data.SqlClient;
using System.Net;
using System.Threading.Tasks;
using Hangfire.Configuration.Pages;
using Microsoft.Owin;

namespace Hangfire.Configuration
{
	public class ConfigurationMiddleware : OwinMiddleware
	{
		private readonly Configuration _configuration;
		private readonly HangfireConfigurationInterfaceOptions _options;
		private CreateServerConfiguration _createServerConfiguration;

		public ConfigurationMiddleware(OwinMiddleware next, HangfireConfigurationInterfaceOptions options) : base(next)
		{
			_options = options;
			if (_options.PrepareSchemaIfNecessary)
				using (var c = new SqlConnection(_options.ConnectionString))
					SqlServerObjectsInstaller.Install(c);
			
			_configuration = new Configuration(
				new ConfigurationRepository(_options.ConnectionString)
			);
			
			_createServerConfiguration = new CreateServerConfiguration();
		}

		public override async Task Invoke(IOwinContext context)
		{
			var page = new ConfigurationPage(_configuration, context.Request.PathBase.Value, _options.AllowNewServerCreation, _createServerConfiguration);
			
			if (context.Request.Path.StartsWithSegments(new PathString("/saveWorkerGoalCount")))
			{
				saveWorkerGoalCount(context.Request, page);
				context.Response.Redirect(context.Request.PathBase.Value);
				return;
			}
			if (context.Request.Path.StartsWithSegments(new PathString("/createNewServerConfiguration")))
			{
				if (createNewServerConfiguration(context.Request, page))
				{
					context.Response.Redirect(context.Request.PathBase.Value);
					return;
				}
			}
			if (context.Request.Path.StartsWithSegments(new PathString("/activateServer")))
			{
				activateServer(context.Request);
				context.Response.Redirect(context.Request.PathBase.Value);
				return;
			}
			
			await renderPage(context.Response, page);
		}
		
		
		private void saveWorkerGoalCount(IOwinRequest request, ConfigurationPage page)
		{
			var workers = tryParseNullable(request.Query["workers"]);
			var configurationId = tryParseNullable(request.Query["configurationId"]);
			_configuration.WriteGoalWorkerCount(workers, configurationId);
			page.DisplayConfirmationMessage();
		}

		private bool createNewServerConfiguration(IOwinRequest request, ConfigurationPage page)
		{
			_createServerConfiguration.Server = request.Query["server"];
			_createServerConfiguration.Database = request.Query["database"];
			_createServerConfiguration.User = request.Query["user"];
			_createServerConfiguration.Password = request.Query["password"];
			_createServerConfiguration.SchemaName = request.Query["schemaName"];
			_createServerConfiguration.UserForCreate = request.Query["userForCreate"];
			_createServerConfiguration.PasswordForCreate = request.Query["passwordForCreate"];

			try
			{
				_configuration.CreateServerConfiguration(_createServerConfiguration);
				return true;
			}
			catch (Exception ex)
			{
				page.DisplayErrorMessage(ex.Message);
				return false;
			}
		}

		private void activateServer(IOwinRequest request)
		{
			var id = int.Parse(request.Query["configurationId"]);
			_configuration.ActivateServer(id);
		}

		private static async Task renderPage(IOwinResponse response, ConfigurationPage page)
		{
			var html = page.ToString();
			response.StatusCode = (int) HttpStatusCode.OK;
			response.ContentType = "text/html";
			await response.WriteAsync(html);
		}

		private int? tryParseNullable(string value) => 
			int.TryParse(value, out var outValue) ? (int?) outValue : null;
	}
}