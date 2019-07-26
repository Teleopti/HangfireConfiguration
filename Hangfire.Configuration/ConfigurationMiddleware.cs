using System;
using System.Data.SqlClient;
using System.Linq;
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

			if (context.Request.Path.StartsWithSegments(new PathString("/saveConfig")))
			{
				var workers = tryParseNullable(context.Request.Query["workers"]);
				_configuration.WriteGoalWorkerCount(workers);
				page.DisplayConfirmationMessage();
			}
			
			
			if (context.Request.Path.StartsWithSegments(new PathString("/saveNewServerConfiguration")))
			{
				_createServerConfiguration.Server = context.Request.Query["server"];
				_createServerConfiguration.Database = context.Request.Query["database"];
				_createServerConfiguration.User = context.Request.Query["user"];
				_createServerConfiguration.Password = context.Request.Query["password"];
				_createServerConfiguration.SchemaName = context.Request.Query["schemaName"];
				
				var anyConfigurationIsNull = _createServerConfiguration.GetType().GetProperties()
					.Any(p => (string)p.GetValue(_createServerConfiguration) == "");

				if (anyConfigurationIsNull)
					page.DisplayInvalidConfigurationMessage();
				else
					_configuration.CreateServerConfiguration(_createServerConfiguration);
			}
			
			if (context.Request.Path.StartsWithSegments(new PathString("/activateConfiguration")))
			{
				var id = int.Parse(context.Request.Query["id"]);
				_configuration.ActivateServer(id);
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