using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Hangfire.Configuration.Pages;
using Microsoft.Owin;
using Newtonsoft.Json.Linq;

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

			if (context.Request.Path.Value.Equals("/saveWorkerGoalCount"))
			{
				saveWorkerGoalCount(context);
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

			if (context.Request.Path.Value.Equals("/activateServer"))
			{
				activateServer(context);
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


		private void saveWorkerGoalCount(IOwinContext context)
		{
			var parsed = ParseRequestBody(context.Request);
			var configurationId = tryParseNullable(parsed.SelectToken("configurationId").Value<string>());
			var workers = tryParseNullable(parsed.SelectToken("workers").Value<string>());
			
			_configuration.WriteGoalWorkerCount(workers, configurationId);
			
			context.Response.StatusCode = (int) HttpStatusCode.OK;
			context.Response.ContentType = "text/html";
			context.Response.Write("Worker goal count was saved successfully!");
		}

		private static JObject ParseRequestBody(IOwinRequest request)
		{
			string text;
			using (var reader = new StreamReader(request.Body))
				text = reader.ReadToEnd();
			var parsed = JObject.Parse(text);
			return parsed;
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

		private void activateServer(IOwinContext context)
		{
			var parsed = ParseRequestBody(context.Request);
			var configurationId = parsed.SelectToken("configurationId").Value<int>();
			_configuration.ActivateServer(configurationId);
			context.Response.StatusCode = (int) HttpStatusCode.OK;
		}

	
		private int? tryParseNullable(string value) => 
			int.TryParse(value, out var outValue) ? (int?) outValue : null;
	}
}