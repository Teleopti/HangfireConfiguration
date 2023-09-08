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
        private readonly Lazy<IEnumerable<(Func<HttpContext, bool> matcher, Action<HttpContext> action)>> _routes;
        
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

            _routes = new Lazy<IEnumerable<(Func<HttpContext, bool> matcher, Action<HttpContext> action)>>(routes);
        }

        private IEnumerable<(Func<HttpContext, bool> matcher, Action<HttpContext> action)> routes()
        {
            yield return (c => c.Request.Method == "GET" && string.IsNullOrEmpty(c.Request.Path.Value), displayPage);
            yield return (c => c.Request.Path.Value.Equals("/nothing"), _ => {});
            yield return (c => c.Request.Method == "GET", returnResource);
            yield return (c => c.Request.Path.Value.Equals("/saveWorkerGoalCount"), saveWorkerGoalCount);
            yield return (c => c.Request.Path.Value.Equals("/saveMaxWorkersPerServer"), saveMaxWorkersPerServer);
            yield return (c => c.Request.Path.Value.Equals("/createNewServerConfiguration"), createNewServerConfiguration);
            yield return (c => c.Request.Path.Value.Equals("/activateServer"), activateServer);
            yield return (c => c.Request.Path.Value.Equals("/inactivateServer"), inactivateServer);
            yield return (c => c.Request.Path.Value.Equals("/enableWorkerBalancer"), enableWorkerBalancer);
            yield return (c => c.Request.Path.Value.Equals("/disableWorkerBalancer"), disableWorkerBalancer);
        }

        public Task Invoke(HttpContext context)
        {
            var syncIoFeature = context.Features.Get<IHttpBodyControlFeature>();
            if (syncIoFeature != null)
                syncIoFeature.AllowSynchronousIO = true;

            handleRequest(context);

            return Task.CompletedTask;
        }

        private void handleRequest(HttpContext context)
        {
            var authorized = _options.Authorization?.Authorize(context) ?? true;
            if (!authorized)
            {
                context.Response.StatusCode = (int) HttpStatusCode.Unauthorized;
                return;
            }

            var route = _routes.Value.FirstOrDefault(x => x.matcher.Invoke(context));
            if (route.action != null)
            {
                processRequest(context, () => { route.action.Invoke(context); });
                return;
            }
            
            context.Response.StatusCode = (int) HttpStatusCode.NotFound;
        }
        
        private void returnResource(HttpContext c)
        {
            var resourceName = c.Request.Path.Value
                .TrimStart('/')
                .Replace("_", ".");
            var contentType = resourceName.EndsWith(".js") ? "application/javascript" : "text/css";

            c.Response.StatusCode = (int) HttpStatusCode.OK;
            c.Response.ContentType = contentType;

            using var stream = GetType().Assembly.GetManifestResourceStream($"{typeof(ConfigurationPage).Namespace}.{resourceName}");
            
            if (stream == null)
            {
                c.Response.StatusCode = (int) HttpStatusCode.NotFound;
                return;
            }
            
            stream.CopyTo(c.Response.Body);
        }
        
        private void displayPage(HttpContext context) =>
            display(context, p => p.BuildPage());

        private void saveWorkerGoalCount(HttpContext context)
        {
            var configurationId = parseConfigurationId(context);
            _configurationApi.WriteGoalWorkerCount(new WriteGoalWorkerCount
            {
                ConfigurationId = configurationId,
                Workers = tryParseNullable(context.Request.Form["workers"])
            });
            display(context, p => p.Configuration(configurationId));
            display(context, p => p.Message("Worker goal count was saved successfully!"));
        }

        private void saveMaxWorkersPerServer(HttpContext context)
        {
            var configurationId = parseConfigurationId(context);
            _configurationApi.WriteMaxWorkersPerServer(new WriteMaxWorkersPerServer
            {
                ConfigurationId = configurationId,
                MaxWorkers = tryParseNullable(context.Request.Form["maxWorkers"])
            });
            display(context, p => p.Configuration(configurationId));
            display(context, p => p.Message("Max workers per server was saved successfully!"));
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

        private void inactivateServer(HttpContext context)
        {
            var configurationId = parseConfigurationId(context);
            _configurationApi.InactivateServer(configurationId);
            display(context, p => p.Configuration(configurationId));
        }

        private void activateServer(HttpContext context)
        {
            var configurationId = parseConfigurationId(context);
            _configurationApi.ActivateServer(configurationId);
            display(context, p => p.Configuration(configurationId));
        }

        private void enableWorkerBalancer(HttpContext c)
        {
            var configurationId = parseConfigurationId(c);
            _configurationApi.EnableWorkerBalancer(configurationId);
            display(c, p => p.Configuration(configurationId));
        }

        private void disableWorkerBalancer(HttpContext c)
        {
            var configurationId = parseConfigurationId(c);
            _configurationApi.DisableWorkerBalancer(configurationId);
            display(c, p => p.Configuration(configurationId));
        }

        private void display(HttpContext context, Func<ConfigurationPage, string> html)
        {
            var page = new ConfigurationPage(_configuration, context.Request.PathBase.Value, _options);
            var response = html.Invoke(page);
            context.Response.WriteAsync(response).Wait();
        }

        private int parseConfigurationId(HttpContext context) =>
            int.Parse(context.Request.Form["configurationId"]);
        
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