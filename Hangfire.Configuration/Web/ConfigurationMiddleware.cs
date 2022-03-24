using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Hangfire.Configuration.Internals;
using Newtonsoft.Json.Linq;
#if NETSTANDARD2_0
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
#else
using Microsoft.Owin;

#endif

namespace Hangfire.Configuration.Web
{
#if NETSTANDARD2_0
    public class ConfigurationMiddleware
#else
	public class ConfigurationMiddleware : OwinMiddleware
#endif
	{
		private readonly HangfireConfiguration _configuration;
		private readonly ConfigurationApi _configurationApi;
		private readonly ConfigurationOptions _options;

		public ConfigurationMiddleware(
#if NETSTANDARD2_0
            RequestDelegate next,
#else
			OwinMiddleware next,
#endif
			ConfigurationOptions options,
			IDictionary<string, object> properties)
#if !NETSTANDARD2_0
			: base(next)
#endif
		{
			_configuration = (properties?.ContainsKey("HangfireConfiguration") ?? false)
			    ? (HangfireConfiguration) properties["HangfireConfiguration"]
			    : new HangfireConfiguration();
			
			if (options != null)
				_configuration.UseOptions(options);
			_options = _configuration.Options().ConfigurationOptions();
			
			_configurationApi = _configuration.ConfigurationApi();
			if (_options.PrepareSchemaIfNecessary)
				using (var c = _options.ConnectionString.CreateConnection())
					HangfireConfigurationSchemaInstaller.Install(c);
		}

#if NETSTANDARD2_0
        public async Task Invoke(HttpContext context)
#else
		public override async Task Invoke(IOwinContext context)
#endif
		{
			await handleRequest(context);
		}

#if NETSTANDARD2_0
        private async Task handleRequest(HttpContext context)
        {
	        var syncIoFeature = context.Features.Get<IHttpBodyControlFeature>();
	        if (syncIoFeature != null)
		        syncIoFeature.AllowSynchronousIO = true;

#else
		private async Task handleRequest(IOwinContext context)
		{
#endif

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
				await processRequest(context, async () => await saveWorkerGoalCount(context));
				return;
			}

			if (context.Request.Path.Value.Equals("/saveMaxWorkersPerServer"))
			{
				await processRequest(context, async () => await saveMaxWorkersPerServer(context));
				return;
			}

			if (context.Request.Path.Value.Equals("/createNewServerConfiguration"))
			{
				await processRequest(context, async () => await createNewServerConfiguration(context));
				return;
			}

			if (context.Request.Path.Value.Equals("/activateServer"))
			{
				await processRequest(context, async () => { _configurationApi.ActivateServer(await parseConfigurationId(context)); });
				return;
			}

			if (context.Request.Path.Value.Equals("/inactivateServer"))
			{
				await processRequest(context, async () => { _configurationApi.InactivateServer(await parseConfigurationId(context)); });
				return;
			}

			if (!string.IsNullOrEmpty(context.Request.Path.Value))
			{
				context.Response.StatusCode = (int) HttpStatusCode.NotFound;
				return;
			}

			await processRequest(context, async () =>
			{
				var page = new ConfigurationPage(_configuration, context.Request.PathBase.Value, _options);
				var html = page.ToString();
				await context.Response.WriteAsync(html);
			});
		}

#if NETSTANDARD2_0
        private async Task<int> parseConfigurationId(HttpContext context) =>
#else
		private async Task<int> parseConfigurationId(IOwinContext context) =>
#endif
			(await parseRequestBody(context.Request)).SelectToken("configurationId").Value<int>();

#if NETSTANDARD2_0
        private async Task saveWorkerGoalCount(HttpContext context)
#else
		private async Task saveWorkerGoalCount(IOwinContext context)
#endif
		{
			var parsed = await parseRequestBody(context.Request);
			_configurationApi.WriteGoalWorkerCount(new WriteGoalWorkerCount
			{
				ConfigurationId = tryParseNullable(parsed.SelectToken("configurationId")?.Value<string>()),
				Workers = tryParseNullable(parsed.SelectToken("workers").Value<string>())
			});
			context.Response.WriteAsync("Worker goal count was saved successfully!").Wait();
		}

#if NETSTANDARD2_0
        private async Task saveMaxWorkersPerServer(HttpContext context)
#else
		private async Task saveMaxWorkersPerServer(IOwinContext context)
#endif
		{
			var parsed = await parseRequestBody(context.Request);
			_configurationApi.WriteMaxWorkersPerServer(new WriteMaxWorkersPerServer
			{
				ConfigurationId = parsed.SelectToken("configurationId").Value<int>(),
				MaxWorkers = tryParseNullable(parsed.SelectToken("maxWorkers").Value<string>())
			});
			context.Response.WriteAsync("Max workers per server was saved successfully!").Wait();
		}

#if NETSTANDARD2_0
        private async Task createNewServerConfiguration(HttpContext context)
#else
		private async Task createNewServerConfiguration(IOwinContext context)
#endif
		{
			var parsed = await parseRequestBody(context.Request);
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

#if NETSTANDARD2_0
        private async Task<JObject> parseRequestBody(HttpRequest request)
#else
		private async Task<JObject> parseRequestBody(IOwinRequest request)
#endif
		{
			string text;
			using (var reader = new StreamReader(request.Body))
				text = await reader.ReadToEndAsync();
			if (string.IsNullOrWhiteSpace(text)) throw new ArgumentException("Request empty", nameof(request));

			var parsed = JObject.Parse(text);
			return parsed;
		}

		private static int? tryParseNullable(string value) =>
			int.TryParse(value, out var outValue) ? outValue : null;

#if NETSTANDARD2_0
        private async Task processRequest(HttpContext context, Func<Task> action)
#else
		private async Task processRequest(IOwinContext context, Func<Task> action)
#endif
		{
			try
			{
				context.Response.StatusCode = (int) HttpStatusCode.OK;
				context.Response.ContentType = "text/html";
				await action.Invoke();
			}
			catch (Exception ex)
			{
				context.Response.StatusCode = (int) HttpStatusCode.InternalServerError;
				await context.Response.WriteAsync(ex.Message);
			}
		}
	}
}