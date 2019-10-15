using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Hangfire.Configuration.Pages;
#if NETSTANDARD2_0
using Microsoft.AspNetCore.Http;
#else
using Microsoft.Owin;
#endif
using Newtonsoft.Json.Linq;

namespace Hangfire.Configuration
{
#if NETSTANDARD2_0
    public class ConfigurationMiddleware
#else
    public class ConfigurationMiddleware : OwinMiddleware
#endif
    {
        private readonly ConfigurationOptions _options;
        private readonly HangfireConfiguration _configuration;
        private readonly ConfigurationApi _configurationApi;

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
            _options = options;
            _configuration = HangfireConfiguration.UseHangfireConfiguration(null, _options, properties);
            _configurationApi = _configuration.ConfigurationApi();
            if (_options.PrepareSchemaIfNecessary)
                using (var c = new SqlConnection(_options.ConnectionString))
                    SqlServerObjectsInstaller.Install(c);
        }

#if NETSTANDARD2_0
        public Task Invoke(HttpContext context)
#else
        public override Task Invoke(IOwinContext context)
#endif
        {
            handleRequest(context);
            return Task.CompletedTask;
        }

#if NETSTANDARD2_0
        private void handleRequest(HttpContext context)
#else
        private void handleRequest(IOwinContext context)
#endif
        {
            if (context.Request.Path.Value.Equals("/script"))
            {
                context.Response.StatusCode = (int) HttpStatusCode.OK;
                context.Response.ContentType = "application/javascript";
                using (var stream = GetType().Assembly.GetManifestResourceStream($"{typeof(ConfigurationPage).Namespace}.script.js"))
                    stream.CopyTo(context.Response.Body);
                return;
            }

            if (context.Request.Path.Value.Equals("/styles"))
            {
                context.Response.StatusCode = (int) HttpStatusCode.OK;
                context.Response.ContentType = "text/css";
                using (var stream = GetType().Assembly.GetManifestResourceStream($"{typeof(ConfigurationPage).Namespace}.styles.css"))
                    stream.CopyTo(context.Response.Body);
                return;
            }

            if (context.Request.Path.Value.Equals("/saveWorkerGoalCount"))
            {
                processRequest(context, () => saveWorkerGoalCount(context));
                return;
            }

            if (context.Request.Path.Value.Equals("/createNewServerConfiguration"))
            {
                processRequest(context, () => createNewServerConfiguration(context));
                return;
            }

            if (context.Request.Path.Value.Equals("/activateServer"))
            {
                processRequest(context, () =>
                {
                    var parsed = parseRequestBody(context.Request);
                    var configurationId = parsed.SelectToken("configurationId").Value<int>();
                    _configurationApi.ActivateServer(configurationId);
                });
                return;
            }

            processRequest(context, () =>
            {
                var page = new ConfigurationPage(_configuration, context.Request.PathBase.Value, _options);
                var html = page.ToString();
                context.Response.WriteAsync(html).Wait();
            });
        }


#if NETSTANDARD2_0
        private void saveWorkerGoalCount(HttpContext context)
#else
        private void saveWorkerGoalCount(IOwinContext context)
#endif
        {
            var parsed = parseRequestBody(context.Request);
            _configurationApi.WriteGoalWorkerCount(new WriteGoalWorkerCount
            {
                ConfigurationId = tryParseNullable(parsed.SelectToken("configurationId")?.Value<string>()),
                Workers = tryParseNullable(parsed.SelectToken("workers").Value<string>())
            });
            context.Response.WriteAsync("Worker goal count was saved successfully!").Wait();
        }

#if NETSTANDARD2_0
        private void createNewServerConfiguration(HttpContext context)
#else
        private void createNewServerConfiguration(IOwinContext context)
#endif
        {
            var parsed = parseRequestBody(context.Request);
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
            _configurationApi.CreateServerConfiguration(configuration);
        }

#if NETSTANDARD2_0
        private JObject parseRequestBody(HttpRequest request)
#else
        private JObject parseRequestBody(IOwinRequest request)
#endif
        {
            string text;
            using (var reader = new StreamReader(request.Body))
                text = reader.ReadToEnd();
            var parsed = JObject.Parse(text);
            return parsed;
        }

        private int? tryParseNullable(string value) =>
            int.TryParse(value, out var outValue) ? (int?) outValue : null;

#if NETSTANDARD2_0
        private void processRequest(HttpContext context, Action action)
#else
        private void processRequest(IOwinContext context, Action action)
#endif
        {
            try
            {
                context.Response.StatusCode = (int) HttpStatusCode.OK;
                context.Response.ContentType = "text/html";
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