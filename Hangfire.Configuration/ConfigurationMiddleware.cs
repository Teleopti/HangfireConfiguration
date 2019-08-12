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
				new ConfigurationRepository(_options.ConnectionString),
				new HangfireSchemaCreator()
			);
			
			_createServerConfiguration = new CreateServerConfiguration();
		}

		public override Task Invoke(IOwinContext context)
		{
			handleRequest(context);
			return Task.CompletedTask;
		}

		private void handleRequest(IOwinContext context)
		{
			var page = new ConfigurationPage(_configuration, context.Request.PathBase.Value, _options.AllowNewServerCreation, _createServerConfiguration);

			if (context.Request.Path.StartsWithSegments(new PathString("/saveWorkerGoalCount")))
			{
				saveWorkerGoalCount(context.Request, page);
				context.Response.Redirect(context.Request.PathBase.Value + "/savedConfiguration");
				return;
			}

			if (context.Request.Path.StartsWithSegments(new PathString("/createNewServerConfiguration")))
			{
				if (createNewServerConfiguration(context.Request, page))
				{
					context.Response.Redirect(context.Request.PathBase.Value + "/savedConfiguration");
					return;
				}
			}

			if (context.Request.Path.StartsWithSegments(new PathString("/activateServer")))
			{
				activateServer(context.Request);
				context.Response.Redirect(context.Request.PathBase.Value);
				return;
			}

			if (context.Request.Path.StartsWithSegments(new PathString("/savedConfiguration")))
			{
				page.DisplayConfirmationMessage();
			}

			var html = page.ToString();
			context.Response.StatusCode = (int) HttpStatusCode.OK;
			context.Response.ContentType = "text/html";
			context.Response.Write(html);
		}


		private void saveWorkerGoalCount(IOwinRequest request, ConfigurationPage page)
		{
			var workers = tryParseNullable(request.Query["workers"]);
			var configurationId = tryParseNullable(request.Query["configurationId"]);
			_configuration.WriteGoalWorkerCount(workers, configurationId);
		}

		private bool createNewServerConfiguration(IOwinRequest request, ConfigurationPage page)
		{
			_createServerConfiguration.Server = request.Query["server"];
			_createServerConfiguration.Database = request.Query["database"];
			_createServerConfiguration.User = request.Query["user"];
			_createServerConfiguration.Password = request.Query["password"];
			_createServerConfiguration.SchemaName = request.Query["schemaName"];
			_createServerConfiguration.SchemaCreatorUser = request.Query["userForCreate"];
			_createServerConfiguration.SchemaCreatorPassword = request.Query["passwordForCreate"];

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

	
		private int? tryParseNullable(string value) => 
			int.TryParse(value, out var outValue) ? (int?) outValue : null;
	}
}