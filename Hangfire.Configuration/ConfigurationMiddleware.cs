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
		private readonly HangfireConfigurationOptions _options;
		private NewStorageConfiguration _newStorageConfiguration;

		public ConfigurationMiddleware(OwinMiddleware next, HangfireConfigurationOptions options) : base(next)
		{
			_options = options;
			if (_options.PrepareSchemaIfNecessary)
				using (var c = new SqlConnection(_options.ConnectionString))
					SqlServerObjectsInstaller.Install(c);
			
			_configuration = new Configuration(
				new ConfigurationRepository(_options.ConnectionString)
			);
			
			_newStorageConfiguration = new NewStorageConfiguration();
		}

		public override async Task Invoke(IOwinContext context)
		{
			var page = new ConfigurationPage(_configuration, context.Request.PathBase.Value, _options.AllowNewStorageCreation, _newStorageConfiguration);

			if (context.Request.Path.StartsWithSegments(new PathString("/saveConfig")))
			{
				var workers = tryParseNullable(context.Request.Query["workers"]);
				_configuration.WriteGoalWorkerCount(workers);
				page.DisplayConfirmationMessage();
			}
			
			
			if (context.Request.Path.StartsWithSegments(new PathString("/saveNewStorageConfiguration")))
			{
				_newStorageConfiguration.Server = context.Request.Query["server"];
				_newStorageConfiguration.Database = context.Request.Query["database"];
				_newStorageConfiguration.User = context.Request.Query["user"];
				_newStorageConfiguration.Password = context.Request.Query["password"];
				_newStorageConfiguration.SchemaName = context.Request.Query["schemaName"];
				
				var anyConfigurationIsNull = _newStorageConfiguration.GetType().GetProperties()
					.Any(p => (string)p.GetValue(_newStorageConfiguration) == "");

				if (anyConfigurationIsNull)
					page.DisplayInvalidConfigurationMessage();
				else
					_configuration.SaveNewStorageConfiguration(_newStorageConfiguration);
			}
			
			if (context.Request.Path.StartsWithSegments(new PathString("/activateConfiguration")))
			{
				var id = int.Parse(context.Request.Query["id"]);
				_configuration.ActivateStorage(id);
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