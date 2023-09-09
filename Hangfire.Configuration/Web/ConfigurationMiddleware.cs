#if NETSTANDARD2_0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Hangfire.Configuration.Internals;
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
            yield return (c => c.Request.Path.Value.Equals("/nothing"), _ => { });
            yield return (c => c.Request.Method == "GET", returnResource);
            yield return (c => c.Request.Path.Value.Equals("/createNewServerSelection"), createNewServerSelection);
            yield return (c => c.Request.Path.Value.Equals("/createNewServerConfiguration"), createNewServerConfiguration);
            yield return (c => c.Request.Path.Value.Equals("/saveWorkerGoalCount"), saveWorkerGoalCount);
            yield return (c => c.Request.Path.Value.Equals("/saveMaxWorkersPerServer"), saveMaxWorkersPerServer);
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

        private void createNewServerSelection(HttpContext context)
        {
            var provider = context.Request.Query["databaseProvider"];
            display(context, p => p.CreateConfiguration(provider));
        }

        private void createNewServerConfiguration(HttpContext context)
        {
            var databaseProvider = context.Request.Form["databaseProvider"];

            if (databaseProvider == "SqlServer" || string.IsNullOrEmpty(databaseProvider))
            {
                _configurationApi.CreateServerConfiguration(new CreateSqlServerWorkerServer
                {
                    Name = context.Request.Form["name"],
                    Server = context.Request.Form["server"],
                    Database = context.Request.Form["database"],
                    User = context.Request.Form["user"],
                    Password = context.Request.Form["password"],
                    SchemaName = context.Request.Form["schemaName"],
                    SchemaCreatorUser = context.Request.Form["schemaCreatorUser"],
                    SchemaCreatorPassword = context.Request.Form["schemaCreatorPassword"]
                });
            }

            if (databaseProvider == "PostgreSql")
            {
                _configurationApi.CreateServerConfiguration(new CreatePostgresWorkerServer
                {
                    Name = context.Request.Form["name"],
                    Server = context.Request.Form["server"],
                    Database = context.Request.Form["database"],
                    User = context.Request.Form["user"],
                    Password = context.Request.Form["password"],
                    SchemaName = context.Request.Form["schemaName"],
                    SchemaCreatorUser = context.Request.Form["schemaCreatorUser"],
                    SchemaCreatorPassword = context.Request.Form["schemaCreatorPassword"],
                });
            }

            if (databaseProvider == "Redis")
            {
                _configurationApi.CreateServerConfiguration(new CreateRedisWorkerServer
                {
                    Name = context.Request.Form["name"],
                    Configuration = context.Request.Form["server"],
                    Prefix = context.Request.Form["schemaName"],
                });
            }

            var configurationId = _configurationApi.ReadConfigurations().Max(x => x.Id.Value);

            display(context, p => p.Configuration(configurationId));
            display(context, p => p.CreateConfigurationSelection());
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
                // Uses a htmx extension to redirect error message into a div placeholder
                // defined by this in the html:
                // hx-ext='response-targets' hx-target-500='next .error'
                // reswap is needed to put the message inside the placeholder
                // couldnt find a neat way to keep these 2 things together.
                context.Response.Headers.Append("HX-Reswap", "innerHTML");
                display(context, p => p.Message(ex.Message));
            }
        }
    }
}

#endif