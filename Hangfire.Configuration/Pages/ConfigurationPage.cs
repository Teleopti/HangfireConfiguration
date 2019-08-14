using System.Linq;
using Hangfire.Dashboard;

namespace Hangfire.Configuration.Pages
{
    public class ConfigurationPage : RazorPage
    {
        private readonly Configuration _configuration;
        private readonly string _basePath;
        private bool _displayConfirmationMessage;
        private bool _displayErrorMessage;
        private readonly bool _allowNewServerCreation;
        private readonly CreateServerConfiguration _inputedServerConfiguration;
        private string _errorMessage;

        public ConfigurationPage(Configuration configuration, string basePath, bool allowNewServerCreation, CreateServerConfiguration inputedServerConfiguration)
        {
            _configuration = configuration;
            _basePath = basePath;
            _allowNewServerCreation = allowNewServerCreation;
            _inputedServerConfiguration = inputedServerConfiguration;
        }

        public void DisplayConfirmationMessage() =>
            _displayConfirmationMessage = true;

        public void DisplayErrorMessage(string message)
        {
            _errorMessage = message;
            _displayErrorMessage = true;
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

            if (_displayConfirmationMessage)
            {
                WriteLiteral(@"<div class='confirm-msg'>Saved configuration successfully!</div>");
            }

            WriteLiteral("<h2>Hangfire configuration</h2>");

            WriteInformationHeader();

            if (_allowNewServerCreation)
                WriteConfigurations(configurations);
            else
                WriteGoalWorkerCountForm(configurations);

            if (_allowNewServerCreation && configurations.Count() < 2)
            {
                WriteCreateConfiguration();
            }

            WriteScripts();
            WriteLiteral("</body>");
            WriteLiteral("</html>");
        }

        private void WriteScripts()
        {
            WriteLiteral($@"
<script>

function saveForm(formId, path, doPostBackOnOk) {{    
    var formElement = document.querySelector('#' + formId);
    var formData = new FormData(formElement);
    var request = new XMLHttpRequest();
    
    request.onload = function() {{
            if (request.status != 200) {{ 
                alert('Error ${{request.status}}: ${{request.statusText}}');
            }} else if ( doPostBackOnOk ) {{ 
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
                        <button type='button' onClick=""saveForm('activateForm_{configuration.Id}', '{_basePath}/activateServer', true)"">Activate configuration</button>
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
                        <button type='button' onClick=""saveForm('workerCountForm_{configuration.Id}', '{_basePath}/saveWorkerGoalCount')"">Submit</button>
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
                        <button type='button' onClick=""saveForm('workerCountForm_{configuration.Id}', '{_basePath}/saveWorkerGoalCount')"">Submit</button>
                    </form>
                </div>
            </fieldset>");
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

        private void WriteCreateConfiguration()
        {
            var errorMsg = "";
            if (_displayErrorMessage)
            {
                errorMsg = $@"&nbsp;&nbsp; <span style='color: red; font-weight: 600'>{_errorMessage}</span>";
            }

            WriteLiteral(
$@"
    <fieldset>
        <legend>Create new Hangfire server</legend>
        <form action='{_basePath}/createNewServerConfiguration'>
            <div class='flex-grid'>
                <div style='width: 240px'>
                    <label for='server'>Server: </label><br>
                    <input type='text' id='server' name='server' value='{_inputedServerConfiguration.Server}' style='width:200px'>
                    <br><label for='database'>Database: </label><br>
                    <input type='text' id='database' name='database' value='{_inputedServerConfiguration.Database}' style='width:200px'>
                    <br><label for='schemaName'>Schema: </label><br>
                    <input type='text' id='schemaName' name='schemaName' value='{_inputedServerConfiguration.SchemaName}' style='width:200px'>
                    <br><label for='user'>User: </label><br>
                    <input type='text' id='user' name='user' value='{_inputedServerConfiguration.User}' style='width:200px'>
                    <br><label for='password'>Password: </label><br>
                    <input type='text' id='password' name='password' value='{_inputedServerConfiguration.Password}' style='width:200px'>
                </div>
                <div class='col'>
                    <label for='userForCreate'>Admin user: </label><br>
                    <input type='text' id='userForCreate' name='userForCreate' value='{_inputedServerConfiguration.SchemaCreatorUser}' style='width:200px'>
                    <br><label for='passwordForCreate'>Admin password: </label><br>
                    <input type='text' id='passwordForCreate' name='passwordForCreate' value='{_inputedServerConfiguration.SchemaCreatorPassword}' style='width:200px'>
                    <br><br><button type='submit'>Create</button>{errorMsg}
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

        .confirm-msg {
            color:green;
            background:#F1F8E9;
            font-size: 20px;
            font-weight:bolder; 
        }
    </style>");

    }
}