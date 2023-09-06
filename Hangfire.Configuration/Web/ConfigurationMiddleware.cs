#if NETSTANDARD2_0

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Hangfire.Configuration.Internals;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace Hangfire.Configuration.Web
{
	public class ConfigurationMiddleware
	{
		private readonly HangfireConfiguration _configuration;
		private readonly ConfigurationApi _configurationApi;
		private readonly ConfigurationOptions _options;

		public ConfigurationMiddleware(
			RequestDelegate next,
			IDictionary<string, object> properties)
		{
			var injected = properties?.ContainsKey("HangfireConfiguration") ?? false;
			if (injected)
			{
				_configuration = (HangfireConfiguration) properties["HangfireConfiguration"];
			}
			else
			{
				_configuration = new HangfireConfiguration();
				if (properties?.ContainsKey("HangfireConfigurationOptions") ?? false)
					_configuration.UseOptions((ConfigurationOptions) properties["HangfireConfigurationOptions"]);
			}

			_options = _configuration.Options().ConfigurationOptions();

			_configurationApi = _configuration.ConfigurationApi();
			if (_options.PrepareSchemaIfNecessary)
				using (var c = _options.ConnectionString.CreateConnection())
					HangfireConfigurationSchemaInstaller.Install(c);
		}

		public async Task Invoke(HttpContext context)
		{
			await handleRequest(context);
		}

		private async Task handleRequest(HttpContext context)
		{
			var syncIoFeature = context.Features.Get<IHttpBodyControlFeature>();
			if (syncIoFeature != null)
				syncIoFeature.AllowSynchronousIO = true;

			if (context.Request.Path.Value.Equals("/script"))
			{
				context.Response.StatusCode = (int) HttpStatusCode.OK;
				context.Response.ContentType = "application/javascript";
				using var stream = GetType().Assembly.GetManifestResourceStream($"{typeof(ConfigurationPage).Namespace}.script.js");
				await stream.CopyToAsync(context.Response.Body);
				return;
			}

			if (context.Request.Path.Value.Equals("/styles"))
			{
				context.Response.StatusCode = (int) HttpStatusCode.OK;
				context.Response.ContentType = "text/css";
				using var stream = GetType().Assembly.GetManifestResourceStream($"{typeof(ConfigurationPage).Namespace}.styles.css");
				await stream.CopyToAsync(context.Response.Body);
				return;
			}

			var authorized = _options.Authorization?.Authorize(context);
			if (authorized.HasValue && !authorized.Value)
			{
				context.Response.StatusCode = (int) HttpStatusCode.Unauthorized;
				return;
			}

			if (context.Request.Path.Value.Equals("/saveWorkerGoalCount"))
			{
				processRequest(context, () => saveWorkerGoalCount(context));
				return;
			}

			if (context.Request.Path.Value.Equals("/saveMaxWorkersPerServer"))
			{
				processRequest(context, () => saveMaxWorkersPerServer(context));
				return;
			}

			if (context.Request.Path.Value.Equals("/createNewServerConfiguration"))
			{
				processRequest(context, () => createNewServerConfiguration(context));
				return;
			}

			if (context.Request.Path.Value.Equals("/activateServer"))
			{
				processRequest(context, () => { _configurationApi.ActivateServer(parseConfigurationId(context)); });
				return;
			}

			if (context.Request.Path.Value.Equals("/inactivateServer"))
			{
				processRequest(context, () => { _configurationApi.InactivateServer(parseConfigurationId(context)); });
				return;
			}

			if (context.Request.Path.Value.Equals("/enableWorkerBalancer"))
			{
				processRequest(context, () => { _configurationApi.EnableWorkerBalancer(parseConfigurationId(context)); });
				return;
			}

			if (context.Request.Path.Value.Equals("/disableWorkerBalancer"))
			{
				processRequest(context, () => { _configurationApi.DisableWorkerBalancer(parseConfigurationId(context)); });
				return;
			}
			
			if (!string.IsNullOrEmpty(context.Request.Path.Value))
			{
				context.Response.StatusCode = (int) HttpStatusCode.NotFound;
				return;
			}

			processRequest(context, async () =>
			{
				var page = new ConfigurationPage(_configuration, context.Request.PathBase.Value, _options);
				var html = page.ToString();
				context.Response.WriteAsync(html).Wait();
			});
		}

		private int parseConfigurationId(HttpContext context) =>
			parseRequestJsonBody(context.Request).SelectToken("configurationId").Value<int>();
		
		private void saveWorkerGoalCount(HttpContext context)
		{
			var parsed = parseRequestJsonBody(context.Request);
			_configurationApi.WriteGoalWorkerCount(new WriteGoalWorkerCount
			{
				ConfigurationId = tryParseNullable(parsed.SelectToken("configurationId")?.Value<string>()),
				Workers = tryParseNullable(parsed.SelectToken("workers").Value<string>())
			});
			context.Response.WriteAsync("Worker goal count was saved successfully!").Wait();
		}

		private void saveMaxWorkersPerServer(HttpContext context)
		{
			var parsed = parseRequestJsonBody(context.Request);
			_configurationApi.WriteMaxWorkersPerServer(new WriteMaxWorkersPerServer
			{
				ConfigurationId = parsed.SelectToken("configurationId").Value<int>(),
				MaxWorkers = tryParseNullable(parsed.SelectToken("maxWorkers").Value<string>())
			});
			context.Response.WriteAsync("Max workers per server was saved successfully!").Wait();
		}

		private void createNewServerConfiguration(HttpContext context)
		{
			var parsed = parseRequestJsonBody(context.Request);
			var provider = parsed.SelectToken("databaseProvider")?.Value<string>();
			if (provider == "PostgreSql")
			{
				_configurationApi.CreateServerConfiguration(new CreatePostgresWorkerServer
				{
					Name = parsed.SelectToken("name")?.Value<string>(),
					Server = parsed.SelectToken("server").Value<string>(),
					Database = parsed.SelectToken("database").Value<string>(),
					User = parsed.SelectToken("user").Value<string>(),
					Password = parsed.SelectToken("password").Value<string>(),
					SchemaName = parsed.SelectToken("schemaName").Value<string>(),
					SchemaCreatorUser = parsed.SelectToken("schemaCreatorUser").Value<string>(),
					SchemaCreatorPassword = parsed.SelectToken("schemaCreatorPassword").Value<string>(),
				});
				return;
			}

			if (provider == "redis")
			{
				_configurationApi.CreateServerConfiguration(new CreateRedisWorkerServer
				{
					Name = parsed.SelectToken("name")?.Value<string>(),
					Configuration = parsed.SelectToken("server").Value<string>(),
					Prefix = parsed.SelectToken("schemaName").Value<string>(),
				});
				return;
			}

			_configurationApi.CreateServerConfiguration(new CreateSqlServerWorkerServer
			{
				Name = parsed.SelectToken("name")?.Value<string>(),
				Server = parsed.SelectToken("server").Value<string>(),
				Database = parsed.SelectToken("database").Value<string>(),
				User = parsed.SelectToken("user").Value<string>(),
				Password = parsed.SelectToken("password").Value<string>(),
				SchemaName = parsed.SelectToken("schemaName").Value<string>(),
				SchemaCreatorUser = parsed.SelectToken("schemaCreatorUser").Value<string>(),
				SchemaCreatorPassword = parsed.SelectToken("schemaCreatorPassword").Value<string>()
			});
		}

		private JObject parseRequestJsonBody(HttpRequest request)
		{
			string text;
			using (var reader = new StreamReader(request.Body))
				text = reader.ReadToEnd();
			if (string.IsNullOrWhiteSpace(text)) throw new ArgumentException("Request empty", nameof(request));

			return JObject.Parse(text);
		}

		private static int? tryParseNullable(string value) =>
			int.TryParse(value, out var outValue) ? outValue : null;

		private void processRequest(HttpContext context, Action action)
		{
			try
			{
				context.Response.StatusCode = (int) HttpStatusCode.OK;
				context.Response.ContentType = "text/html; charset=utf-8";
				action.Invoke();
			}
			catch (Exception ex)
			{
				context.Response.StatusCode = (int) HttpStatusCode.InternalServerError;
				context.Response.WriteAsync(ex.Message).Wait();
			}
		}
	}
}

#endif