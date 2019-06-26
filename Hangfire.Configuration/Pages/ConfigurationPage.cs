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
			WriteLiteral("\r\n");
			WriteLiteral("\r\n");
			WriteLiteral($@"<form  action=""{_basePath}/saveConfig"">
							<label for=""workers"">Workers: </label>
							<input type=""number"" value=""{getWorkers()}"" id =""workers"" name=""workers"" style='width:60px'>
							<button type=""submit"">Submit</button>");
			if (_displayConfirmationMessage)
				WriteLiteral("&nbsp;&nbsp; <span>Configuration was saved !</span>");	
			
		}

		private string getWorkers() => 
			_configuration.ReadGoalWorkerCount().ToString();
		
	}
}