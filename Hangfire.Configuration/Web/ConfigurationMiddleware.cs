#if NETSTANDARD2_0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Hangfire.Configuration.Internals;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace Hangfire.Configuration.Web;

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
		if (properties == null || !properties.ContainsKey("HangfireConfiguration"))
			throw new InvalidOperationException(
				"UseHangfireConfigurationUI must be called after UseHangfireConfiguration. " +
				"Call app.UseHangfireConfiguration(options) before app.UseHangfireConfigurationUI(path).");
		
		_configuration = (HangfireConfiguration) properties["HangfireConfiguration"];
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
		yield return (c => c.Request.Path.Value.Equals("/activateServer"), activateServer);
		yield return (c => c.Request.Path.Value.Equals("/inactivateServer"), inactivateServer);
		yield return (c => c.Request.Path.Value.Equals("/saveContainer"), saveContainer);
		yield return (c => c.Request.Path.Value.Equals("/addContainer"), addContainer);
		yield return (c => c.Request.Path.Value.Equals("/removeContainer"), removeContainer);
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

	private void createNewServerSelection(HttpContext context)
	{
		var provider = context.Request.Query["databaseProvider"];
		display(context, p => p.BuildCreateConfiguration(provider));
	}

	private void createNewServerConfiguration(HttpContext context)
	{
		var databaseProvider = context.Request.Form["databaseProvider"];

		if (databaseProvider == "SqlServer" || string.IsNullOrEmpty(databaseProvider))
		{
			_configurationApi.CreateServerConfiguration(new CreateSqlServer
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
			_configurationApi.CreateServerConfiguration(new CreatePostgresServer
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
			_configurationApi.CreateServerConfiguration(new CreateRedisServer
			{
				Name = context.Request.Form["name"],
				Configuration = context.Request.Form["server"],
				Prefix = context.Request.Form["schemaName"],
			});
		}

		var configurationId = _configurationApi.ReadConfigurations().Max(x => x.Id.Value);

		display(context, p => p.BuildConfiguration(configurationId));
		display(context, p => p.BuildCreateConfiguration(null));
	}

	private void inactivateServer(HttpContext context)
	{
		var configurationId = parseConfigurationId(context);
		_configurationApi.InactivateServer(configurationId);
		display(context, p => p.BuildConfiguration(configurationId));
	}

	private void activateServer(HttpContext context)
	{
		var configurationId = parseConfigurationId(context);
		_configurationApi.ActivateServer(configurationId);
		display(context, p => p.BuildConfiguration(configurationId));
	}

	private void saveContainer(HttpContext c)
	{
		var configurationId = parseConfigurationId(c);
		var containerIndex = tryParseNullable(c.Request.Form["containerIndex"]);
		var command = new WriteContainer
		{
			ConfigurationId = configurationId,
			ContainerIndex = containerIndex.GetValueOrDefault(),
			WorkerBalancerEnabled = c.Request.Form["workerBalancerEnabled"] == "on",
			Workers = tryParseNullable(c.Request.Form["workers"]),
			MaxWorkersPerServer = tryParseNullable(c.Request.Form["maxWorkersPerServer"])
		};
		if (_options.EnableContainerManagement)
		{
			var queues = c.Request.Form["queues"]
				.SelectMany(q => q.Split(','))
				.Select(q => q.Trim())
				.Where(q => !string.IsNullOrEmpty(q))
				.ToArray();
			command.Tag = c.Request.Form["tag"].ToString();
			command.Queues = queues;
		}
		
		_configurationApi.WriteContainer(command);
		
		display(c, p => p.BuildContainer(configurationId, command.ContainerIndex));
		display(c, p => p.Message("Container saved successfully!"));
	}

	private void addContainer(HttpContext c)
	{
		var configurationId = parseConfigurationId(c);
		var tag = c.Request.Form["tag"].ToString();
		if (string.IsNullOrEmpty(tag))
			throw new ArgumentException("Tag is required when adding a container.");
		_configurationApi.AddContainer(configurationId, tag);
		display(c, p => p.BuildConfiguration(configurationId));
		display(c, p => p.Message("Container saved successfully!"));
	}

	private void removeContainer(HttpContext c)
	{
		var configurationId = parseConfigurationId(c);
		var containerIndex = int.Parse(c.Request.Form["containerIndex"]);
		_configurationApi.RemoveContainer(configurationId, containerIndex);
		display(c, p => p.BuildConfiguration(configurationId));
		display(c, p => p.Message("Removed saved successfully!"));
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

#endif