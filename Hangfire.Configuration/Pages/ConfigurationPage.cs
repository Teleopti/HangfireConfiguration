using Hangfire.Dashboard;

namespace Hangfire.Configuration.Pages
{
	public class ConfigurationPage: RazorPage
	{
		private readonly Configuration _configuration;
		private readonly string _basePath;
		private bool _displayConfirmationMessage;

		public ConfigurationPage(Configuration configuration, string basePath)
		{
			_configuration = configuration;
			_basePath = basePath;
		}

		public void DisplayConfirmationMessage() => 
			_displayConfirmationMessage = true;
		
		public override void Execute() => 
			buildHtml();
		
		private void buildHtml()
		{
			WriteLiteral("\r\n");
			WriteLiteral("<h2>Hangfire worker configuration</h2>");
			WriteLiteral("<p>");
			WriteLiteral("Use this configuration to set the goal number of workers to dynamically scale workers per server.");
			WriteLiteral("<br>");
			WriteLiteral("On start up of each Hangfire server, the server will be assigned a number of workers approximate for equal distribution of the goal workers count.");
			WriteLiteral("<br>");
			WriteLiteral("This is an approximation for reasons like: the number of existing servers is not exact, rounding, minimum 1 worker assigned.");
			WriteLiteral("<br>");
			WriteLiteral("As the servers randomly reset, the goal will eventually be met.");
			WriteLiteral("<br>");
			WriteLiteral("Default goal is 10.");
			WriteLiteral("</p>");
			WriteLiteral("\r\n");
			WriteLiteral("\r\n");
			WriteLiteral($@"<form  action=""{_basePath}/saveConfig"">
							<label for=""workers"">Worker Goal Count: </label>
							<input type=""number"" value=""{getWorkers()}"" id =""workers"" name=""workers"" style='width:60px'>
							<button type=""submit"">Submit</button>");
			if (_displayConfirmationMessage)
				WriteLiteral("&nbsp;&nbsp; <span>Configuration was saved !</span>");	
			
		}

		private string getWorkers() => 
			_configuration.ReadGoalWorkerCount().ToString();
		
	}
}