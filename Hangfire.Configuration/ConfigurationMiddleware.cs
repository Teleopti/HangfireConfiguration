using System;
using System.Data.SqlClient;
using System.IO;
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

		public ConfigurationMiddleware(OwinMiddleware next, HangfireConfigurationInterfaceOptions options, CompositionRoot compositionRoot) : base(next)
		{
			_options = options;
			if (_options.PrepareSchemaIfNecessary)
				using (var c = new SqlConnection(_options.ConnectionString))
					SqlServerObjectsInstaller.Install(c);

			compositionRoot = compositionRoot ?? new CompositionRoot();
			_configuration = compositionRoot.BuildConfiguration(_options.ConnectionString);
		}

		public override Task Invoke(IOwinContext context)
		{
			handleRequest(context);
			return Task.CompletedTask;
		}

		private void handleRequest(IOwinContext context)
		{
			if (context.Request.Path.Value.Equals("/postForm.js"))
			{
				context.Response.StatusCode = (int) HttpStatusCode.OK;
				context.Response.ContentType = "application/javascript";
				context.Response.Write($@"
var forms = document.querySelectorAll('form');
forms.forEach(function(form) {{
               
var button = form.querySelector('button');
button.addEventListener('click', function(){{
      var formData = new FormData(form);
      var reloadOnOk = false;
      if (form.attributes['data-reload'])
         reloadOnOk = !!form.attributes['data-reload'].value;
        var request = new XMLHttpRequest();
        request.onload = function() {{
            if (request.status != 200) {{
                alert('Error: ' + request.status + ' : ' + request.response);
            }} else if ( reloadOnOk ) {{
                window.location.reload(true);
            }} else {{
                alert(request.response);
            }}
        }};
        let jsonObject = {{}};
        for (const [key, value]  of formData.entries()) {{
            jsonObject[key] = value;
        }}
        request.open('POST', form.action);
        request.send(JSON.stringify(jsonObject));
    }});
}});
");
				return;
			}
			
			var page = new ConfigurationPage(_configuration, context.Request.PathBase.Value, _options.AllowNewServerCreation);

			if (context.Request.Path.Value.Equals("/saveWorkerGoalCount"))
			{
				saveWorkerGoalCount(context);
				return;
			}

			if (context.Request.Path.Value.Equals("/createNewServerConfiguration"))
			{
				createNewServerConfiguration(context);
				return;
			}

			if (context.Request.Path.Value.Equals("/activateServer"))
			{
				activateServer(context);
				return;
			}
			
			var html = page.ToString();
			context.Response.StatusCode = (int) HttpStatusCode.OK;
			context.Response.ContentType = "text/html";
			context.Response.Write(html);
		}


		private void saveWorkerGoalCount(IOwinContext context)
		{
			var parsed = parseRequestBody(context.Request);
			
			_configuration.WriteGoalWorkerCount(new WriteGoalWorkerCount
			{
				ConfigurationId =  tryParseNullable(parsed.SelectToken("configurationId")?.Value<string>()),
				Workers = tryParseNullable(parsed.SelectToken("workers").Value<string>())
			});
			
			context.Response.StatusCode = (int) HttpStatusCode.OK;
			context.Response.ContentType = "text/html";
			context.Response.Write("Worker goal count was saved successfully!");
		}
		
		private void createNewServerConfiguration(IOwinContext context)
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

			try
			{
				_configuration.CreateServerConfiguration(configuration);
				context.Response.StatusCode = (int) HttpStatusCode.OK;
			}
			catch (Exception ex)
			{
				context.Response.StatusCode = (int) HttpStatusCode.InternalServerError;
				context.Response.Write(ex.Message);
			}
		}
		
		private void activateServer(IOwinContext context)
		{
			var parsed = parseRequestBody(context.Request);
			var configurationId = parsed.SelectToken("configurationId").Value<int>();
			_configuration.ActivateServer(configurationId);
			context.Response.StatusCode = (int) HttpStatusCode.OK;
		}
		
		private static JObject parseRequestBody(IOwinRequest request)
		{
			string text;
			using (var reader = new StreamReader(request.Body))
				text = reader.ReadToEnd();
			var parsed = JObject.Parse(text);
			return parsed;
		}
		

	
		private int? tryParseNullable(string value) => 
			int.TryParse(value, out var outValue) ? (int?) outValue : null;
	}
}