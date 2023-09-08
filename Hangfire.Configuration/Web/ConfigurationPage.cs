using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hangfire.Configuration.Web;

public class ConfigurationPage
{
    private readonly ViewModelBuilder _viewModelBuilder;
    private readonly string _basePath;
    private readonly ConfigurationOptions _options;
    private readonly StringBuilder _content = new();

    public ConfigurationPage(
        HangfireConfiguration configuration,
        string basePath,
        ConfigurationOptions options)
    {
        _viewModelBuilder = configuration.ViewModelBuilder();
        _basePath = basePath;
        _options = options;
    }

    public string BuildPage()
    {
        var configurations = _viewModelBuilder.BuildServerConfigurations().ToArray();
        writePage(configurations);
        return _content.ToString();
    }

    private void writePage(IEnumerable<ViewModel> configurations)
    {
        write("<html>");
        write($@"<base href=""{_basePath}/"">");
        write("<head>");
        write(@"<link rel=""stylesheet"" type=""text/css"" href=""styles_css""/>");
        write("</head>");
        write("<body>");
        write("<h2>Hangfire configuration</h2>");

        configurations = configurations.Any() ? configurations : new[] {new ViewModel()};

        write("<div class='configurations'>");
        foreach (var configuration in configurations)
            writeConfiguration(configuration);
        writeCreateConfiguration();
        write("</div>");

        write($@"<script src=""{_basePath}/script_js""></script>");
        write($@"<script src=""{_basePath}/htmx_min_js""></script>");
        write("</body>");
        write("</html>");
    }

    public string Configuration(int configurationId)
    {
        var configurations = _viewModelBuilder.BuildServerConfigurations().ToArray();
        var configuration = configurations.Single(x => x.Id == configurationId);
        writeConfiguration(configuration);
        return _content.ToString();
    }
    
    private void writeConfiguration(ViewModel configuration)
    {
        var title = "Configuration";
        if (configuration.Name != null)
            title = title + " - " + configuration.Name;
        var state = configuration.Active ? " - <span class='active'>⬤</span> Active" : " - <span class='inactive'>⬤</span> Inactive";

        write($@"
                <div class='configuration'>
                    <fieldset>
                        <legend>{title}{state}</legend>");

        write($"<div><label>Connection string:</label><span>{configuration.ConnectionString}</span></div>");
        if (!string.IsNullOrEmpty(configuration.SchemaName))
        {
            write($"<div><label>Schema name:</label><span>{configuration.SchemaName}</span></div>");
        }

        writeActivateConfiguration(configuration);

        writeWorkerBalancer(configuration);

        write(@"</fieldset></div>");
    }

    private void writeWorkerBalancer(ViewModel configuration)
    {
        var enabled = configuration.WorkerBalancerEnabled ? " - <span class='enabled'>⬤</span> Enabled" : " - <span class='disabled'>⬤</span> Disabled";
        var action = configuration.WorkerBalancerEnabled ? "disableWorkerBalancer" : "enableWorkerBalancer";
        var button = configuration.WorkerBalancerEnabled ? "Disable" : "Enable";

        write($@"
                    <fieldset>
                        <legend>Worker balancer{enabled}</legend>
						");

        // Math.Min(Environment.ProcessorCount * 5, 20)
        write($@"
		        <div>
					<form class='form' hx-post='{action}' hx-target='closest .configuration' hx-swap='outerHTML'>
						<label style='width: 126px'>Worker balancer: </label>
						<input type='hidden' value='{configuration.Id}' name='configurationId'>
						<button class='button' type='submit'>{button}</button>
						(When disabled, hangfire default will be used) 
					</form>
				</div>");

        write($@"
                <div>
                    <form class='form' hx-post='saveWorkerGoalCount' hx-target='closest .configuration' hx-swap='outerHTML'>
                        <label for='workers' style='width: 126px'>Worker goal count: </label>
                        <input type='hidden' value='{configuration.Id}' name='configurationId'>
                        <input type='number' value='{configuration.Workers}' name='workers' style='margin-right: 6px; width:60px'>
                        <button class='button' type='submit'>Save</button>
                            (Default: {_options.WorkerBalancerOptions.DefaultGoalWorkerCount}, Max: {_options.WorkerBalancerOptions.MaximumGoalWorkerCount})
							(Temporary configuration, may reset to default at times)
                    </form>
                </div>");

        write($@"
                <div>
                    <form class='form' hx-post='saveMaxWorkersPerServer' hx-target='closest .configuration' hx-swap='outerHTML'>
                        <label for='maxWorkers' style='width: 126px'>Max workers per server: </label>
                        <input type='hidden' value='{configuration.Id}' name='configurationId'>
                        <input type='number' maxlength='3' value='{configuration.MaxWorkersPerServer}' name='maxWorkers' style='margin-right: 6px; width:60px'>
                        <button class='button' type='submit'>Save</button>
                    </form>
                </div>");

        write($@"
                    </fieldset>
                        ");
    }

    private void writeActivateConfiguration(ViewModel configuration)
    {
        var action = configuration.Active ? "inactivateServer" : "activateServer";
        var button = configuration.Active ? "Inactivate configuration" : "Activate configuration";

        write($@"
                <div>
                    <form class='form' hx-post='{action}' hx-target='closest .configuration' hx-swap='outerHTML' style='margin: 10px'>
                        <input type='hidden' value='{configuration.Id}' name='configurationId'>
                        <button class='button' type='submit'>{button}</button>
                    </form>
                </div>");
    }

    private void writeCreateConfiguration()
    {
        write(
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
    
    public string Message(string message) =>
        $@"
<div class='message' hx-get='nothing' hx-trigger='load delay:2s' hx-swap='delete' hx-target='this'>
    <p>{message}</p>
</div>
";

    private void write(string textToAppend) =>
        _content.Append(textToAppend);
}