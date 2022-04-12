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
		var state = "";
		if (configuration.Active.HasValue)
			state = configuration.Active.GetValueOrDefault() ? 
				" - <span class='active'>⬤</span> Active" : 
				" - <span class='inactive'>⬤</span> Inactive";

		WriteLiteral($@"
                <div class='configuration'>
                    <fieldset>
                        <legend>{title}{state}</legend>");

		WriteLiteral($"<div><label>Connection string:</label><span>{configuration.ConnectionString}</span></div>");
		if (!string.IsNullOrEmpty(configuration.SchemaName))
		{
			WriteLiteral($"<div><label>Schema name:</label><span>{configuration.SchemaName}</span></div>");
		}

		WriteLiteral($@"
                <div>
                    <form class='form' id=""workerCountForm_{configuration.Id}"" action='saveWorkerGoalCount' style='margin-bottom: 3px'>
                        <label for='workers' style='width: 126px'>Worker goal count: </label>
                        <input type='hidden' value='{configuration.Id}' id='configurationId' name='configurationId'>
                        <input type='number' value='{configuration.Workers}' id='workers' name='workers' style='margin-right: 6px; width:60px'>
                        <button class='button' type='button'>Submit</button>
                            (Default: {_options.WorkerDeterminerOptions.DefaultGoalWorkerCount}, Max: {_options.WorkerDeterminerOptions.MaximumGoalWorkerCount})
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


		if (configuration.Active == false)
		{
			WriteLiteral($@"
                    <div>
                        <form class='form' id=""activateForm_{configuration.Id}"" action='activateServer' data-reload='true'>
                            <input type='hidden' value='{configuration.Id}' id='configurationId' name='configurationId'>
                            <button class='button' type='button'>Activate configuration</button>
                        </form>
                    </div>");
		}

		if (configuration.Active == true)
		{
			WriteLiteral($@"
                    <div>
                        <form class='form' id=""inactivateForm_{configuration.Id}"" action='inactivateServer' data-reload='true'>
                            <input type='hidden' value='{configuration.Id}' id='configurationId' name='configurationId'>
                            <button class='button' type='button'>Inactivate configuration</button>
                        </form>
                    </div>");
		}

		WriteLiteral(@"</fieldset></div>");
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
        <button class='button' type='button'>Create</button>
    </form>
</fieldset>
</div>
");
	}
}