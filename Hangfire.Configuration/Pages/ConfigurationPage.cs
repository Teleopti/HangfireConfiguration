using System.Collections.Generic;
using System.Linq;
using Hangfire.Dashboard;

namespace Hangfire.Configuration.Pages
{
    public class ConfigurationPage : RazorPage
    {
        private readonly ViewModelBuilder _viewModelBuilder;
        private readonly string _basePath;
        private readonly HangfireConfigurationUIOptions _options;

        public ConfigurationPage(HangfireConfiguration configuration, string basePath, HangfireConfigurationUIOptions options)
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
    <legend>Information</legend>
    <p>
        <bold>*Worker goal count:</bold> Configuration value to set the goal number of workers to dynamically scale
        workers per server.<br>On start up of each Hangfire server, the server will be assigned a number of workers approximate
        for equal distribution of the goal workers count.<br>This is an approximation for reasons like: the number of existing servers is
        not exact, rounding, minimum 1 worker assigned.<br>As the servers randomly reset, the goal will eventually be
        met.<br>Default goal is 10 if no value is specified
    </p>
</fieldset>");
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
                    <div><span class='configLabel'>Server:</span><span>{configuration.ServerName}</span></div>
                    <div><span class='configLabel'>Database:</span><span>{configuration.DatabaseName}</span></div>
                    <div><span class='configLabel'>Schema name:</span><span>{configuration.SchemaName}</span></div>");
            }

            WriteLiteral($@"
                <div>
                    <form class='form' id=""workerCountForm_{configuration.Id}"" action='saveWorkerGoalCount'>
                        <label for='workers'>Worker goal count: </label>
                        <input type='hidden' value='{configuration.Id}' id='configurationId' name='configurationId'>
                        <input type='number' value='{configuration.Workers}' id='workers' name='workers' style='margin-right: 6px; width:60px'>
                        <button class='button' type='button'>Submit</button>
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
            <div style='width: 240px'>
                <label for='server'>Server: </label><br>
                <input type='text' id='server' name='server'  style='width:200px'>
                <br><label for='database'>Database: </label><br>
                <input type='text' id='database' name='database' style='width:200px'>
                <br><label for='schemaName'>Schema: </label><br>
                <input type='text' id='schemaName' name='schemaName' style='width:200px'>
                <br><label for='user'>User: </label><br>
                <input type='text' id='user' name='user' style='width:200px'>
                <br><label for='password'>Password: </label><br>
                <input type='text' id='password' name='password' style='width:200px'>
            </div>
            <div class='col'>
                <label for='schemaCreatorUser'>Creator user: </label><br>
                <input type='text' id='schemaCreatorUser' name='schemaCreatorUser' style='width:200px'>
                <br><label for='schemaCreatorPassword'>Creator password: </label><br>
                <input type='text' id='schemaCreatorPassword' name='schemaCreatorPassword' style='width:200px'>
                <br><br><button class='button' type='button'>Create</button>
            </div>
        </div>
    </form>
</fieldset>");
        }
    }
}