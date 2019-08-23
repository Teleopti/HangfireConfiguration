using System.Linq;
using Hangfire.Dashboard;

namespace Hangfire.Configuration.Pages
{
    public class ConfigurationPage : RazorPage
    {
        private readonly Configuration _configuration;
        private readonly string _basePath;
        private readonly bool _allowNewServerCreation;

        public ConfigurationPage(Configuration configuration, string basePath, bool allowNewServerCreation)
        {
            _configuration = configuration;
            _basePath = basePath;
            _allowNewServerCreation = allowNewServerCreation;
        }

        public override void Execute()
        {
            var configurations = _configuration.BuildServerConfigurations().ToArray();
            buildHtml(configurations);
        }

        private void buildHtml(ServerConfigurationViewModel[] configurations)
        {
            WriteLiteral("<html>");
            WriteLiteral($@"<base href=""{_basePath}/"">");
            WriteHead();
            WriteLiteral("<body>");
            WriteLiteral("<h2>Hangfire configuration</h2>");

            WriteInformationHeader();

            if (_allowNewServerCreation)
            {
                WriteConfigurations(configurations);
                if (configurations.Count() < 2)
                {
                    WriteCreateConfiguration();
                }
            }
            else
                WriteGoalWorkerCountForm(configurations);
            
            WriteScripts();
            WriteLiteral("</body>");
            WriteLiteral("</html>");
        }

        private void WriteScripts()
        {
            WriteLiteral($@"
<script src='{_basePath}/postForm.js'>
</script>");
        }

        private void WriteHead()
        {
            WriteLiteral("<head>");
            WriteStyle();
            WriteLiteral("</head>");
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
        
        private void WriteConfigurations(ServerConfigurationViewModel[] configurations)
        {
            if (configurations.Any())
            {
                WriteLiteral("<div class='flex-grid'>");

                foreach (var configuration in configurations)
                {
                    WriteConfiguration(configuration);
                }

                WriteLiteral("</div>");
            }
            else
            {
                WriteConfiguration(new ServerConfigurationViewModel());
            }
        }

        private void WriteConfiguration(ServerConfigurationViewModel configuration)
        {
            var activateForm = "";

            if (configuration.Active == "Inactive")
            {
                activateForm = $@"              
<div>    
    <form class='form' id=""activateForm_{configuration.Id}"" action='activateServer' data-reload='true'>
        <input type='hidden' value='{configuration.Id}' id='configurationId' name='configurationId'>
        <button class='button' type='button'>Activate configuration</button>
    </form>
</div>";
            }

            WriteLiteral($@"        
<div class='col'>
    <fieldset style='height: 190px'>
        <legend>{configuration.Title} - {configuration.Active}</legend>
        <div><span class='configLabel'>Server:</span><span>{configuration.ServerName}</span></div>
        <div><span class='configLabel'>Database:</span><span>{configuration.DatabaseName}</span></div>
        <div><span class='configLabel'>Schema name:</span><span>{configuration.SchemaName}</span></div>
        <div>
            <form class='form' id=""workerCountForm_{configuration.Id}"" action='saveWorkerGoalCount'>
                <label for='workers'>Worker goal count: </label>
                <input type='hidden' value='{configuration.Id}' id='configurationId' name='configurationId'>
                <input type='number' value='{configuration.Workers}' id='workers' name='workers' style='margin-right: 6px; width:60px'>
                <button class='button' type='button'>Submit</button>
            </form>
        </div>
        {activateForm}
    </fieldset>
</div>");
        }

        private void WriteGoalWorkerCountForm(ServerConfigurationViewModel[] configurations)
        {
            var configuration = configurations?.FirstOrDefault() ?? new ServerConfigurationViewModel();

            WriteLiteral($@"        
<fieldset>
    <legend>Hangfire worker goal configuration</legend>
    <div>
        <form class='form' id=""workerCountForm_{configuration.Id}"" action='saveWorkerGoalCount'>
            <label for='workers'>Worker goal count: </label>
            <input type='hidden' value='{configuration.Id}' id='configurationId' name='configurationId'>
            <input type='number' value='{configuration.Workers}' id='workers' name='workers' style='margin-right: 6px; width:60px'>
            <button class='button' type='button'>Submit</button>
        </form>
    </div>
</fieldset>");
        }
        
        private void WriteCreateConfiguration()
        {
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

        private void WriteStyle() =>
            WriteLiteral(
                @"
<style>
    body {
        font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
        margin-left: 20px
    }

    h2 {
        font-size: 30px;
        font-weight: 500
    }

    div {
        padding-top: 3px
    }

    .configLabel {
        padding-right: 10px;
        padding-top: 30px;
        color: #888;
        font-weight: bold;
    }

    p {
        font-size: 12px;
        line-height: 14px;
    }

    label {
        padding: 10px 0px 10px;
        color: #888;
        font-weight: bold;
    }

    form.input {
        width: 200px
    }

    button {
        padding: 1px 5px;
        font-size: 12px;
        line-height: 1.5;
        border-radius: 3px;
        color: #fff;
        background-color: #337ab7;
        border: 1px solid transparent;
    }

    fieldset {
        -webkit-border-radius: 8px;
        -moz-border-radius: 8px;
        border-radius: 8px;
        border: 1px solid #428bca;
        line-height: 1.5em;
    }

    legend {
        font-size: 16px;
        font-weight: bold;
    }

    .flex-grid {
        display: flex;
    }

    .col {
        flex: 1;
    }
</style>");
    }
}