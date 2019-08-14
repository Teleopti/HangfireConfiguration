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
<script>

function postForm(formId, path, reloadOnOk) {{    
    var formElement = document.querySelector('#' + formId);
    var formData = new FormData(formElement);
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
    request.open('POST', path);
    //request.setRequestHeader('Content-Type', 'application/json;charset=UTF-8');
    request.send(JSON.stringify(jsonObject));
}}
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
    <form id=""activateForm_{configuration.Id}"">
        <input type='hidden' value='{configuration.Id}' id='configurationId' name='configurationId'>
        <button type='button' onClick=""postForm('activateForm_{configuration.Id}', '{_basePath}/activateServer', true)"">Activate configuration</button>
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
            <form id=""workerCountForm_{configuration.Id}"">
                <label for='workers'>Worker goal count: </label>
                <input type='hidden' value='{configuration.Id}' id='configurationId' name='configurationId'>
                <input type='number' value='{configuration.Workers}' id='workers' name='workers' style='margin-right: 6px; width:60px'>
                <button type='button' onClick=""postForm('workerCountForm_{configuration.Id}', '{_basePath}/saveWorkerGoalCount')"">Submit</button>
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
        <form id=""workerCountForm_{configuration.Id}"">
            <label for='workers'>Worker goal count: </label>
            <input type='hidden' value='{configuration.Id}' id='configurationId' name='configurationId'>
            <input type='number' value='{configuration.Workers}' id='workers' name='workers' style='margin-right: 6px; width:60px'>
            <button type='button' onClick=""postForm('workerCountForm_{configuration.Id}', '{_basePath}/saveWorkerGoalCount')"">Submit</button>
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
    <form id=""createForm"">
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
                <br><br><button type='button' onClick=""postForm('createForm', '{_basePath}/createNewServerConfiguration', true)"">Create</button>
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