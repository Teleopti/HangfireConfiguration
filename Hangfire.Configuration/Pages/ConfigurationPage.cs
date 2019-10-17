using System.Collections.Generic;
using System.Linq;
using Hangfire.Dashboard;

namespace Hangfire.Configuration.Pages
{
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

            WriteInformationHeader();

            configurations = configurations.Any() ? configurations : new[] {new ViewModel()};
            
            WriteLiteral("<div class='flex-grid'>");
            foreach (var configuration in configurations)
                WriteConfiguration(configuration);
            WriteLiteral("</div>");

            if (_options.AllowNewServerCreation)
                WriteCreateConfiguration(configurations);

            WriteLiteral($@"<script src='{_basePath}/script'></script>");
            WriteLiteral("</body>");
            WriteLiteral("</html>");
        }

        private void WriteInformationHeader()
        {
            WriteLiteral(@"
                <fieldset>
                    <legend>Information</legend>");
            if (_options.AllowNewServerCreation)
                WriteStorageActivationInformation();
            WriteWorkerGoalCountInformation();
            WriteLiteral(@"</fieldset>");
        }

        private void WriteStorageActivationInformation()
        {
            WriteLiteral(@"
                <h3>Activate configuration</h3>
                 <p>
                    When active configuration is changed, the jobs will eventually be put on queue for the active one.<br>
                    The active configuration will be the one receiving the new jobs, but the inactive one will continue processing the old jobs already queued.
                </p>");
        }
        
        private void WriteWorkerGoalCountInformation()
        {
            WriteLiteral(@"
                <h3>Worker goal count</h3>
                <p>
                    Configuration value to set the goal number of workers to dynamically scale
                    workers per server.<br>On start up of each Hangfire server, the server will be assigned a number of workers approximate
                    for equal distribution of the goal workers count.<br>This is an approximation for reasons like: the number of existing servers is
                    not exact, rounding, minimum 1 worker assigned.<br>As the servers randomly reset, the goal will eventually be
                    met.<br>Default goal is 10 if no value is specified
                 </p>");
        }

        private void WriteConfiguration(ViewModel configuration)
        {
            WriteLiteral($@"
                <div class='col'>
                    <fieldset>
                        <legend>Configuration {(configuration.Active != null ? " - " + configuration.Active : null)}</legend>");

            if (!string.IsNullOrEmpty(configuration.ServerName))
            {
                WriteLiteral($@"
                    <div><label>Server:</label><span>{configuration.ServerName}</span></div>
                    <div><label>Database:</label><span>{configuration.DatabaseName}</span></div>
                    <div><label>Schema name:</label><span>{configuration.SchemaName}</span></div>");
            }

            WriteLiteral($@"
                <div>
                    <form class='form' id=""workerCountForm_{configuration.Id}"" action='saveWorkerGoalCount'>
                        <label for='workers'>Worker goal count: </label>
                        <input type='hidden' value='{configuration.Id}' id='configurationId' name='configurationId'>
                        <input type='number' value='{configuration.Workers}' id='workers' name='workers' style='margin-right: 6px; width:60px'>
                        <button class='button' type='button'>Submit</button>
                            (Default: {_options.DefaultGoalWorkerCount}, Max: {_options.MaximumGoalWorkerCount})
                    </form>
                </div>");

            if (configuration.Active == "Inactive")
            {
                WriteLiteral($@"
                    <div>
                        <form class='form' id=""activateForm_{configuration.Id}"" action='activateServer' data-reload='true'>
                            <input type='hidden' value='{configuration.Id}' id='configurationId' name='configurationId'>
                            <button class='button' type='button'>Activate configuration</button>
                        </form>
                    </div>");
            }

            WriteLiteral($@"</fieldset></div>");
        }

        private void WriteCreateConfiguration(IEnumerable<ViewModel> configurations)
        {
            if (configurations.Count() >= 2)
                return;

            WriteLiteral(
                $@"
<fieldset>
    <legend>Create new Hangfire server</legend>
    <form class='form' id=""createForm"" action='createNewServerConfiguration' data-reload='true'>
        <div class='flex-grid'>
            <fieldset>
                <h3>SQL server</h3>
                <label for='server'>Server: </label><br>
                <input type='text' id='server' name='server'><br>
                <label for='database'>Database (existing): </label><br>
                <input type='text' id='database' name='database'><br>
                <label for='schemaName'>Schema (optional): </label><br>
                <input type='text' id='schemaName' name='schemaName'>
             </fieldset>
             <fieldset>
                <h3>Hangfire worker server credentials</h3>
                <label for='user'>SQL User Name (existing):</label><br>
                <input type='text' id='user' name='user'><br>
                <label for='password'>SQL Password: </label><br>
                <input type='password' id='password' name='password'>
            </fieldset>
            <fieldset>
                <h3>Database credentials to create Hangfire storage</h3>
                <label for='schemaCreatorUser'>Creator user: </label><br>
                <input type='text' id='schemaCreatorUser' name='schemaCreatorUser'><br>
                <label for='schemaCreatorPassword'>Creator password: </label><br>
                <input type='password' id='schemaCreatorPassword' name='schemaCreatorPassword'><br><br>
                <button class='button' type='button'>Create</button>
            </fieldset>
        </div>
    </form>
</fieldset>");
        }
    }
}