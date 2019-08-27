using System;
using System.Data.SqlClient;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Hangfire.Configuration.Pages;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;

namespace Hangfire.Configuration
{
	public class ConfigurationMiddleware
	{
		private readonly Configuration _configuration;
        private readonly RequestDelegate _next;
        private readonly HangfireConfigurationInterfaceOptions _options;

		public ConfigurationMiddleware(RequestDelegate next, HangfireConfigurationInterfaceOptions options, CompositionRoot compositionRoot)
		{
            _next = next;
            _options = options;
			if (_options.PrepareSchemaIfNecessary)
				using (var c = new SqlConnection(_options.ConnectionString))
					SqlServerObjectsInstaller.Install(c);

			compositionRoot = compositionRoot ?? new CompositionRoot();
			_configuration = compositionRoot.BuildConfiguration(_options.ConnectionString);
		}

		public async Task Invoke(HttpContext context)
		{
			await handleRequest(context);
		}

		private async Task handleRequest(HttpContext context)
		{
			if (context.Request.Path.Value.Equals("/script"))
			{
				context.Response.StatusCode = (int) HttpStatusCode.OK;
				context.Response.ContentType = "application/javascript";
				using(var stream = GetType().Assembly.GetManifestResourceStream($"{typeof(ConfigurationPage).Namespace}.script.js"))
					stream.CopyTo(context.Response.Body);
                return;
			}
			
			if (context.Request.Path.Value.Equals("/styles"))
			{
				context.Response.StatusCode = (int) HttpStatusCode.OK;
				context.Response.ContentType = "text/css";
				using(var stream = GetType().Assembly.GetManifestResourceStream($"{typeof(ConfigurationPage).Namespace}.styles.css"))
					stream.CopyTo(context.Response.Body);
                return;
			}
			
			var page = new ConfigurationPage(_configuration, context.Request.PathBase.Value, _options.AllowNewServerCreation);

			if (context.Request.Path.Value.Equals("/saveWorkerGoalCount"))
			{
				await saveWorkerGoalCount(context);
                return;
			}

			if (context.Request.Path.Value.Equals("/createNewServerConfiguration"))
			{
				await createNewServerConfiguration(context);
                return;
			}

			if (context.Request.Path.Value.Equals("/activateServer"))
			{
				await activateServer(context);
                return;
			}
			
			var html = page.ToString();
			context.Response.StatusCode = (int) HttpStatusCode.OK;
			context.Response.ContentType = "text/html";
			await context.Response.WriteAsync(html);
		}


		private async Task saveWorkerGoalCount(HttpContext context)
		{
			var parsed = await parseRequestBody(context.Request);
			
			_configuration.WriteGoalWorkerCount(new WriteGoalWorkerCount
			{
				ConfigurationId =  tryParseNullable(parsed.SelectToken("configurationId")?.Value<string>()),
				Workers = tryParseNullable(parsed.SelectToken("workers").Value<string>())
			});
			
			context.Response.StatusCode = (int) HttpStatusCode.OK;
			context.Response.ContentType = "text/html";
			await context.Response.WriteAsync("Worker goal count was saved successfully!");
		}
		
		private async Task createNewServerConfiguration(HttpContext context)
		{
			var parsed = await parseRequestBody(context.Request);
			var configuration = new CreateServerConfiguration
			{
				Server = parsed.SelectToken("server").Value<string>(),
				Database = parsed.SelectToken("database").Value<string>(),
				User = parsed.SelectToken("user").Value<string>(),
				Password = parsed.SelectToken("password").Value<string>(),
				SchemaName = parsed.SelectToken("schemaName").Value<string>(),
				SchemaCreatorUser = parsed.SelectToken("schemaCreatorUser").Value<string>(),
				SchemaCreatorPassword = parsed.SelectToken("schemaCreatorPassword").Value<string>()
			};

			try
			{
				_configuration.CreateServerConfiguration(configuration);
				context.Response.StatusCode = (int) HttpStatusCode.OK;
			}
			catch (Exception ex)
			{
				context.Response.StatusCode = (int) HttpStatusCode.InternalServerError;
				await context.Response.WriteAsync(ex.Message);
			}
		}
		
		private async Task activateServer(HttpContext context)
		{
			var parsed = await parseRequestBody(context.Request);
			var configurationId = parsed.SelectToken("configurationId").Value<int>();
			_configuration.ActivateServer(configurationId);
			context.Response.StatusCode = (int) HttpStatusCode.OK;
		}
		
		private static async Task<JObject> parseRequestBody(HttpRequest request)
		{
			string text;
			using (var reader = new StreamReader(request.Body))
				text = await reader.ReadToEndAsync();
			var parsed = JObject.Parse(text);
			return parsed;
		}
		
		private int? tryParseNullable(string value) => 
			int.TryParse(value, out var outValue) ? (int?) outValue : null;
	}
}