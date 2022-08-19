using System.Linq;
using Hangfire.Dashboard;

namespace Hangfire.Configuration.Web;

public class ConfigurationPage : RazorPage
{
	private readonly ViewModelBuilder _viewModelBuilder;
	private readonly string _basePath;
	private readonly ConfigurationOptions _options;

	public ConfigurationPage(HangfireConfiguration configuration, string basePath, ConfigurationOptions options)
	{
		_viewModelBuilder = configuration.ViewModelBuilder();
		_basePath = basePath;
		_options = options;
	}

	public override void Execute()
	{
		var configurations = _viewModelBuilder.BuildServerConfigurations().ToArray();
		buildHtml(configurations);
	}

	private void buildHtml(ViewModel[] configurations)
	{
		WriteLiteral("<html>");
		WriteLiteral($@"<base href=""{_basePath}/"">");
		WriteLiteral("<head>");
		WriteLiteral(@"<link rel=""stylesheet"" type=""text/css"" href=""styles""/>");
		WriteLiteral("</head>");
		WriteLiteral("<body>");
		WriteLiteral("<h2>Hangfire configuration</h2>");

		configurations = configurations.Any() ? configurations : new[] {new ViewModel()};

		WriteLiteral("<div class='configurations'>");
		foreach (var configuration in configurations)
			writeConfiguration(configuration);
		writeCreateConfiguration();
		WriteLiteral("</div>");

		WriteLiteral($@"<script src='{_basePath}/script'></script>");
		WriteLiteral("</body>");
		WriteLiteral("</html>");
	}

	private void writeConfiguration(ViewModel configuration)
	{
		var title = "Configuration";
		if (configuration.Name != null)
			title = title + " - " + configuration.Name;
		var state = configuration.Active ? " - <span class='active'>⬤</span> Active" : " - <span class='inactive'>⬤</span> Inactive";

		WriteLiteral($@"
                <div class='configuration'>
                    <fieldset>
                        <legend>{title}{state}</legend>");

		WriteLiteral($"<div><label>Connection string:</label><span>{configuration.ConnectionString}</span></div>");
		if (!string.IsNullOrEmpty(configuration.SchemaName))
		{
			WriteLiteral($"<div><label>Schema name:</label><span>{configuration.SchemaName}</span></div>");
		}

		writeActivateConfiguration(configuration);

		writeWorkerBalancer(configuration);

		WriteLiteral(@"</fieldset></div>");
	}

	private void writeWorkerBalancer(ViewModel configuration)
	{
		var enabled = configuration.WorkerBalancerEnabled ? " - <span class='enabled'>⬤</span> Enabled" : " - <span class='disabled'>⬤</span> Disabled";
		var enableAction = configuration.WorkerBalancerEnabled ? "disableWorkerBalancer" : "enableWorkerBalancer";
		var enableButton = configuration.WorkerBalancerEnabled ? "Disable" : "Enable";

		WriteLiteral($@"
                    <fieldset>
                        <legend>Worker balancer{enabled}</legend>
						");

		// Math.Min(Environment.ProcessorCount * 5, 20)
		WriteLiteral($@"
		        <div>
					<form class='form' id=""workerBalancerEnableForm_{configuration.Id}"" action='{enableAction}' data-reload='true'>
						<label style='width: 126px'>Worker balancer: </label>
						<input type='hidden' value='{configuration.Id}' id='configurationId' name='configurationId'>
						<button class='button' type='button'>{enableButton}</button>
						(When disabled, hangfire default will be used) 
					</form>
				</div>");

		WriteLiteral($@"
                <div>
                    <form class='form' id=""workerCountForm_{configuration.Id}"" action='saveWorkerGoalCount'>
                        <label for='workers' style='width: 126px'>Worker goal count: </label>
                        <input type='hidden' value='{configuration.Id}' id='configurationId' name='configurationId'>
                        <input type='number' value='{configuration.Workers}' id='workers' name='workers' style='margin-right: 6px; width:60px'>
                        <button class='button' type='button'>Submit</button>
                            (Default: {_options.WorkerBalancerOptions.DefaultGoalWorkerCount}, Max: {_options.WorkerBalancerOptions.MaximumGoalWorkerCount})
							(Temporary configuration, may reset to default at times)
                    </form>
                </div>");

		WriteLiteral($@"
                <div>
                    <form class='form' id=""maxWorkersPerServerForm_{configuration.Id}"" action='saveMaxWorkersPerServer'>
                        <label for='maxWorkers' style='width: 126px'>Max workers per server: </label>
                        <input type='hidden' value='{configuration.Id}' id='configurationId' name='configurationId'>
                        <input type='number' maxlength='3' value='{configuration.MaxWorkersPerServer}' id='maxWorkers' name='maxWorkers' style='margin-right: 6px; width:60px'>
                        <button class='button' type='button'>Submit</button>
                    </form>
                </div>");

		WriteLiteral($@"
                    </fieldset>
                        ");
	}

	private void writeActivateConfiguration(ViewModel configuration)
	{
		var action = configuration.Active ? "inactivateServer" : "activateServer";
		var button = configuration.Active ? "Inactivate configuration" : "Activate configuration";

		WriteLiteral($@"
                <div>
                    <form class='form' id=""activateForm_{configuration.Id}"" action='{action}' data-reload='true' style='margin: 10px'>
                        <input type='hidden' value='{configuration.Id}' id='configurationId' name='configurationId'>
                        <button class='button' type='button'>{button}</button>
                    </form>
                </div>");
	}

	private void writeCreateConfiguration()
	{
		WriteLiteral(
			@"
<div class='configuration'>
<fieldset>
    <legend>Create new</legend>
    <form class='form' id=""createForm"" action='createNewServerConfiguration' data-reload='true'>
        <div style='display: flex'>
            <fieldset>
                <h3>Storage</h3>
                <label for='databaseProvider'>Database provider: </label><br>
                <select id='databaseProvider' name='databaseProvider'>
					<option value='SqlServer' selected='true'>SQL Server</option>
					<option value='PostgreSql'>PostgreSql</option>
					<option value='redis'>Redis</option>
				</select><br>
				<label for='server'>Server: </label><br>
                <input type='text' id='server' name='server'><br>
				<div id='database'>
					<label for='database'>Database (existing): </label><br>
					<input type='text' id='database' name='database'><br>
				</div>
				<label for='schemaName'>Schema (optional): </label><br>
				<input type='text' id='schemaName' name='schemaName'>
             </fieldset>
			<div id='applicationUser'>
				<fieldset>
					<h3>Application user</h3>
					<label for='user'>SQL User Name:</label><br>
					<input type='text' id='user' name='user' class='small'><br>
					<label for='password'>SQL Password: </label><br>
					<input type='password' id='password' name='password' class='small'>
				</fieldset>
			</div>
			<div id='creatorUser'>
	            <fieldset>
					<h3>User with create permissions</h3>
					<label for='schemaCreatorUser'>SQL User Name: </label><br>
					<input type='text' id='schemaCreatorUser' name='schemaCreatorUser' class='small'><br>
					<label for='schemaCreatorPassword'>SQL Password: </label><br>
					<input type='password' id='schemaCreatorPassword' name='schemaCreatorPassword' class='small'>
	            </fieldset>
			</div>
        </div>
		<br><br>
        <button class='button' type='button'>Create</button> (<b>Note!</b> Restart is needed!)
    </form>
</fieldset>
</div>
");
	}
}