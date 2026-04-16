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

    internal ConfigurationPage(
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

    public string Configuration(int configurationId)
    {
        var configurations = _viewModelBuilder.BuildServerConfigurations().ToArray();
        var configuration = configurations.Single(x => x.Id == configurationId);
        writeConfiguration(configuration);
        return _content.ToString();
    }

    public string CreateConfiguration(string databaseProvider)
    {
        writeCreateNewServerConfiguration(databaseProvider);
        return _content.ToString();
    }

    private void writePage(IEnumerable<ViewModel> configurations)
    {
        write("<html>");
        write($@"<base href=""{_basePath}/"">");
        write("<head>");
        write(@"<link rel=""stylesheet"" type=""text/css"" href=""styles_css""/>");
        write("</head>");
        write("<body hx-ext='response-targets' hx-target-500='next .error'>");
        write("<h2>Hangfire configuration</h2>");

        configurations = configurations.Any() ? configurations : new[] {new ViewModel()};

        write("<div class='configurations'>");
        foreach (var configuration in configurations)
        {
            writeConfiguration(configuration);
            write("<div class='error'></div>");
        }

        writeCreateNewServerConfiguration(null);
        write("<div class='error'></div>");
        
        write("</div>");
        write("<br/><br/><br/><br/><br/>");

        write($@"<script src=""{_basePath}/htmx_min_js""></script>");
        write($@"<script src=""{_basePath}/response-targets_js""></script>");
        write("</body>");
        write("</html>");
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
            write($"<div><label>Schema name:</label><span>{configuration.SchemaName}</span></div>");

        writeActivateConfiguration(configuration);

        if (_options.EnableContainerManagement)
            writeContainerManagement(configuration);
        else
            writeContainer(configuration);

        write(@"</fieldset></div>");
    }

    private void writeContainer(ViewModel configuration)
    {
        var container = configuration.Containers[0];
        var checkedAttr = container.WorkerBalancerEnabled ? "checked" : "";

        write($@"
                    <fieldset>
                        <legend>Container</legend>
<form hx-post='saveContainer' hx-target='closest .configuration' hx-swap='outerHTML'>
    <input type='hidden' value='{configuration.Id}' name='configurationId'>
    <div>
        <label for='workerBalancerEnabled' style='width: 126px'>Worker balancer: </label>
        <input type='checkbox' name='workerBalancerEnabled' id='workerBalancerEnabled' {checkedAttr}>
        (When disabled, hangfire default will be used)
    </div>
    <div>
        <label for='workers' style='width: 126px'>Worker goal count: </label>
        <input type='number' value='{container.Workers}' name='workers' style='margin-right: 6px; width:60px'>
        (Default: {_options.WorkerBalancerOptions.DefaultGoalWorkerCount}, Max: {_options.WorkerBalancerOptions.MaximumGoalWorkerCount})
    </div>
    <div>
        <label for='maxWorkersPerServer' style='width: 126px'>Max workers per server: </label>
        <input type='number' maxlength='3' value='{container.MaxWorkersPerServer}' name='maxWorkersPerServer' style='margin-right: 6px; width:60px'>
    </div>
    <div style='margin: 10px; margin-bottom: 5px'>
        <button class='button' type='submit'>Save</button>
    </div>
</form>
                    </fieldset>
                        ");
    }

    private void writeContainerManagement(ViewModel configuration)
    {
        var containers = configuration.Containers;
        var availableQueues = configuration.AvailableQueues;

        foreach (var (container, i) in containers.Select((c, i) => (c, i)))
        {
            var checkedAttr = container.WorkerBalancerEnabled ? "checked" : "";
            var legend = string.IsNullOrEmpty(container.Tag) ? "Container" : $"Container - {container.Tag}";
            var containerQueues = container.Queues ?? new string[0];

            write($@"
                    <fieldset>
                        <legend>{legend}</legend>
<form hx-post='saveContainer' hx-target='closest .configuration' hx-swap='outerHTML'>
    <input type='hidden' value='{configuration.Id}' name='configurationId'>
    <input type='hidden' value='{i}' name='containerIndex'>
    <div>
        <label for='tag_{i}' style='width: 126px'>Tag: </label>
        <input type='text' value='{container.Tag}' name='tag' id='tag_{i}' style='width: 200px'>
    </div>
    <div>
        <label style='width: 126px'>Queues: </label>");

            foreach (var queue in availableQueues)
            {
                var queueChecked = containerQueues.Contains(queue) ? "checked" : "";
                write($@"
        <label><input type='checkbox' name='queues' value='{queue}' {queueChecked}> {queue}</label>");
            }

            write($@"
    </div>
    <div>
        <label for='workerBalancerEnabled_{i}' style='width: 126px'>Worker balancer: </label>
        <input type='checkbox' name='workerBalancerEnabled' id='workerBalancerEnabled_{i}' {checkedAttr}>
        (When disabled, hangfire default will be used)
    </div>
    <div>
        <label for='workers_{i}' style='width: 126px'>Worker goal count: </label>
        <input type='number' value='{container.Workers}' name='workers' id='workers_{i}' style='margin-right: 6px; width:60px'>
        (Default: {_options.WorkerBalancerOptions.DefaultGoalWorkerCount}, Max: {_options.WorkerBalancerOptions.MaximumGoalWorkerCount})
    </div>
    <div>
        <label for='maxWorkersPerServer_{i}' style='width: 126px'>Max workers per server: </label>
        <input type='number' maxlength='3' value='{container.MaxWorkersPerServer}' name='maxWorkersPerServer' id='maxWorkersPerServer_{i}' style='margin-right: 6px; width:60px'>
    </div>
    <div style='display: flex; gap: 6px; margin: 10px; margin-bottom: 5px'>
        <button class='button' type='submit'>Save</button>");

            if (i > 0)
            {
                write($@"
        <button class='button' type='button'
            hx-post='removeContainer'
            hx-target='closest .configuration'
            hx-swap='outerHTML'
            hx-vals='{{""configurationId"": ""{configuration.Id}"", ""containerIndex"": ""{i}""}}'>Remove</button>");
            }

            write(@"
    </div>
</form>
                    </fieldset>");
        }

        write($@"
<form hx-post='addContainer' hx-target='closest .configuration' hx-swap='outerHTML'>
    <input type='hidden' value='{configuration.Id}' name='configurationId'>
    <div style='margin: 10px; margin-bottom: 5px'>
        <button class='button' type='submit'>Add container</button>
    </div>
</form>");
    }

    private void writeActivateConfiguration(ViewModel configuration)
    {
        var action = configuration.Active ? "inactivateServer" : "activateServer";
        var button = configuration.Active ? "Inactivate configuration" : "Activate configuration";

        write($@"
                <div>
                    <form hx-post='{action}' hx-target='closest .configuration' hx-swap='outerHTML' style='margin: 10px'>
                        <input type='hidden' value='{configuration.Id}' name='configurationId'>
                        <button class='button' type='submit'>{button}</button>
                    </form>
                </div>");
    }

    private void writeCreateNewServerConfiguration(string databaseProvider)
    {
        write(@$"
<div class='configuration'>
    <fieldset>
        <legend>Create new</legend>
        <div style='display: flex; margin:10px'>
            <button class='button' type='button' hx-post='createNewServerSelection?databaseProvider=SqlServer' hx-target='closest .configuration' hx-swap='outerHTML' style='margin-right: 6px; width:120px'>
                Sql Server
            </button>
            <button class='button' type='button' hx-post='createNewServerSelection?databaseProvider=PostgreSql' hx-target='closest .configuration' hx-swap='outerHTML' style='margin-right: 6px; width:120px'>
                PostgreSql
            </button>
            <button class='button' type='button' hx-post='createNewServerSelection?databaseProvider=Redis' hx-target='closest .configuration' hx-swap='outerHTML' style='margin-right: 6px; width:120px'>
                Redis
            </button>
        </div>
");

        if (databaseProvider != null)
            writeCreateNewServerConfigurationForm(databaseProvider);

        write(@$"</fieldset></div>");
    }

    private void writeCreateNewServerConfigurationForm(string databaseProvider)
    {
        var storageName = "Sql Server";
        if (databaseProvider != "SqlServer")
            storageName = databaseProvider;

        var database = $@"
            <label for='database'>Database (existing): </label><br>
        	<input type='text' id='database' name='database'><br>";
        var applicationUser = $@"
			<fieldset>
				<legend>Application user</legend>
				<label for='user'>SQL User Name:</label><br>
				<input type='text' id='user' name='user' class='small'><br>
				<label for='password'>SQL Password: </label><br>
				<input type='password' id='password' name='password' class='small'>
			</fieldset>";
        var creatorUser = $@"
            <fieldset>
	            <legend>User with create permissions</legend>
	            <label for='schemaCreatorUser'>SQL User Name: </label><br>
	            <input type='text' id='schemaCreatorUser' name='schemaCreatorUser' class='small'><br>
	            <label for='schemaCreatorPassword'>SQL Password: </label><br>
	            <input type='password' id='schemaCreatorPassword' name='schemaCreatorPassword' class='small'>
            </fieldset>";

        if (databaseProvider == "Redis")
        {
            database = null;
            applicationUser = null;
            creatorUser = null;
        }

        write($@"
<form hx-post='createNewServerConfiguration' hx-target='closest .configuration' hx-swap='outerHTML'>
    <div style='display: flex'>
        <fieldset>
            <legend>{storageName}</legend>
            <input type='hidden' value='{databaseProvider}' name='databaseProvider' />
			<label for='server'>Server: </label><br>
            <input type='text' id='server' name='server'><br>
            {database}
			<label for='schemaName'>Schema (optional): </label><br>
			<input type='text' id='schemaName' name='schemaName'>
        </fieldset>
        {applicationUser}
        {creatorUser}
    </div>
	<br>
    <button class='button' type='submit'>Create</button>
</form>
");
    }

    public string Message(string message) =>
        $@"
<div class='message' hx-get='nothing' hx-trigger='load delay:3s' hx-swap='delete' hx-target='this'>
    <p>{message}</p>
</div>
";

    private void write(string textToAppend) =>
        _content.Append(textToAppend);
}