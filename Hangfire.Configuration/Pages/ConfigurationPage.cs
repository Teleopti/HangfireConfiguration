using System.Collections.Generic;
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

        public override void Execute() =>
            buildHtml();

        private void buildHtml()
        {
            var configurations = getConfigurations().ToArray();

            WriteLiteral("<h2 style='font-family:\"Segoe UI\"; font-size: 30px; font-weight:500; margin-left: 20px'>");
            WriteLiteral("Hangfire configuration");
            WriteLiteral("</h2>");
            if (configurations.Any())
            {
                foreach (var configuration in configurations)
                {
                    if (configuration.ServerName != "")
                    {
                        WriteLiteral("<h3 style='font-family:\"Segoe UI\"; font-size: 26px; font-weight:500; margin:0px 20px 10px'>");
                        WriteLiteral($"{configuration.Title} - {configuration.Id} - {configuration.Active}");
                        WriteLiteral("</h3>");
                        WriteLiteral("<div style='padding: 10px;'>");
                        WriteLiteral("<span style='padding: 10px; color: #888; font-weight: bold;'>");
                        WriteLiteral("Server");
                        WriteLiteral("</span>");
                        WriteLiteral("<span>");
                        WriteLiteral($"{configuration.ServerName}");
                        WriteLiteral("</span>");
                        WriteLiteral("</div>");
                        WriteLiteral("<div style='padding: 10px;'>");
                        WriteLiteral("<span style='padding: 10px; color: #888; font-weight: bold;'>");
                        WriteLiteral("Database");
                        WriteLiteral("</span>");
                        WriteLiteral("<span>");
                        WriteLiteral($"{configuration.DatabaseName}");
                        WriteLiteral("</span>");
                        WriteLiteral("</div>");
                        WriteLiteral("<div style='padding: 10px;'>");
                        WriteLiteral("<span style='padding: 10px; color: #888; font-weight: bold;'>");
                        WriteLiteral("Schema Name");
                        WriteLiteral("</span>");
                        WriteLiteral("<span>");
                        WriteLiteral($"{configuration.SchemaName}");
                        WriteLiteral("</span>");
                        WriteLiteral("</div>");
                        WriteLiteral("\r\n");
                        WriteLiteral("\r\n");
                        if (configuration.Active == "Inactive")
                        {
                            WriteLiteral($@"<form action='{_basePath}/activateServer' '<div style='padding: 10px;'>");
                            WriteLiteral($"<input type='hidden' value='{configuration.Id}' id='configurationId' name='configurationId'>");
                            WriteLiteral("<button type='submit' style='padding: 1px 5px; font-size: 12px; line-height: 1.5; border-radius: 3px; color: #fff; background-color: #337ab7; border: 1px solid transparent;'>");
                            WriteLiteral("Activate configuration");
                            WriteLiteral("</button>");
                            WriteLiteral("</form>");
                        }
                    }

                    WriteLiteral("\r\n");
                    WriteLiteral("<p style='border: 1px solid #428bca; line-height: 1.5em; margin-left: 10px; padding: 10px; width: 50%'>");
                    WriteLiteral("Use the goal worker count configuration to set the goal number of workers to dynamically scale workers per server.");
                    WriteLiteral("<br>");
                    WriteLiteral("On start up of each Hangfire server, the server will be assigned a number of workers approximate for equal distribution of the goal workers count.");
                    WriteLiteral("<br>");
                    WriteLiteral("This is an approximation for reasons like: the number of existing servers is not exact, rounding, minimum 1 worker assigned.");
                    WriteLiteral("<br>");
                    WriteLiteral("As the servers randomly reset, the goal will eventually be met.");
                    WriteLiteral("<br>");
                    WriteLiteral("Default goal is 10.");
                    WriteLiteral("</p>");
                    WriteLiteral("\r\n");
                    WriteLiteral("\r\n");
                    WriteLiteral($@"<form  action='{_basePath}/saveWorkerGoalCount' '<div style='padding: 10px;'>");
                    WriteLiteral("<label for='workers' style='padding: 10px 0px 10px; color: #888; font-weight: bold;'>");
                    WriteLiteral("Goal Worker Count: ");
                    WriteLiteral("</label>");
                    WriteLiteral($"<input type='hidden' value='{configuration.Id}' id='configurationId' name='configurationId'>");
                    WriteLiteral($"<input type='number' value='{configuration.Workers}' id='workers' name='workers' style='margin-right: 6px; width:60px'>");
                    WriteLiteral("<button type='submit' style='padding: 1px 5px; font-size: 12px; line-height: 1.5; border-radius: 3px; color: #fff; background-color: #337ab7; border: 1px solid transparent;'>");
                    WriteLiteral("Submit");
                    WriteLiteral("</button>");
                    WriteLiteral("</form>");
                    if (_displayConfirmationMessage)
                        WriteLiteral("&nbsp;&nbsp; <span>Configuration was saved !</span>");
                }
            }
            else
            {
                WriteLiteral("\r\n");
                    WriteLiteral("<p style='border: 1px solid #428bca; line-height: 1.5em; margin-left: 10px; padding: 10px; width: 50%'>");
                    WriteLiteral("Use the goal worker count configuration to set the goal number of workers to dynamically scale workers per server.");
                    WriteLiteral("<br>");
                    WriteLiteral("On start up of each Hangfire server, the server will be assigned a number of workers approximate for equal distribution of the goal workers count.");
                    WriteLiteral("<br>");
                    WriteLiteral("This is an approximation for reasons like: the number of existing servers is not exact, rounding, minimum 1 worker assigned.");
                    WriteLiteral("<br>");
                    WriteLiteral("As the servers randomly reset, the goal will eventually be met.");
                    WriteLiteral("<br>");
                    WriteLiteral("Default goal is 10.");
                    WriteLiteral("</p>");
                    WriteLiteral("\r\n");
                    WriteLiteral("\r\n");
                    WriteLiteral($@"<form  action='{_basePath}/saveWorkerGoalCount' '<div style='padding: 10px;'>");
                    WriteLiteral("<label for='workers' style='padding: 10px 0px 10px; color: #888; font-weight: bold;'>");
                    WriteLiteral("Goal Worker Count: ");
                    WriteLiteral("</label>");
                    WriteLiteral($"<input type='number' value='' id='workers' name='workers' style='margin-right: 6px; width:60px'>");
                    WriteLiteral("<button type='submit' style='padding: 1px 5px; font-size: 12px; line-height: 1.5; border-radius: 3px; color: #fff; background-color: #337ab7; border: 1px solid transparent;'>");
                    WriteLiteral("Submit");
                    WriteLiteral("</button>");
                    WriteLiteral("</form>");
                    if (_displayConfirmationMessage)
                        WriteLiteral("&nbsp;&nbsp; <span>Configuration was saved !</span>");
            }


            if (_allowNewServerCreation && configurations.Count() < 2)
            {
                WriteLiteral($"<h2 style='font-family:\"Segoe UI\"; font-size: 30px; font-weight:500; margin-left: 10px'>");
                WriteLiteral("Create new Hangfire server");
                WriteLiteral("</h2>");
                WriteLiteral($"<form  action='{_basePath}/createNewServerConfiguration'>");
                WriteLiteral("<label for='server' style='padding: 0px 10px; color: #888; font-weight: bold;'>Server: </label>");
                WriteLiteral("<br>");
                WriteLiteral($"<input type='text' id='server' name='server' value='{_inputedServerConfiguration.Server}' style='margin: 0px 10px 10px; width:200px'>");
                WriteLiteral("<br>");
                WriteLiteral("<label for='database' style='padding: 4px 10px; color: #888; font-weight: bold;'>Database: </label>");
                WriteLiteral("<br>");
                WriteLiteral($"<input type='text' id='database' name='database' value='{_inputedServerConfiguration.Database}' style='margin: 0px 10px 10px; width:200px'>");
                WriteLiteral("<br>");
                WriteLiteral("<label for='schemaName' style='padding: 4px 10px; color: #888; font-weight: bold;'>Schema name: </label>");
                WriteLiteral("<br>");
                WriteLiteral($"<input type='text' id='schemaName' name='schemaName' value='{_inputedServerConfiguration.SchemaName}' style='margin: 0px 10px 10px; width:200px'>");
                WriteLiteral("<br>");                
                WriteLiteral("<label for='user' style='padding: 4px 10px; color: #888; font-weight: bold;'>User: </label>");
                WriteLiteral("<br>");
                WriteLiteral($"<input type='text' id='user' name='user' value='{_inputedServerConfiguration.User}' style='margin: 0px 10px 10px; width:200px'>");
                WriteLiteral("<br>");
                WriteLiteral("<label for='password' style='padding: 4px 10px; color: #888; font-weight: bold;'>Password: </label>");
                WriteLiteral("<br>");
                WriteLiteral($"<input type='text' id='password' name='password' value='{_inputedServerConfiguration.Password}' style='margin: 0px 10px 10px; width:200px'>");
                WriteLiteral("<br>");
                WriteLiteral("<label for='userForCreate' style='padding: 4px 10px; color: #888; font-weight: bold;'>User for creating schema: </label>");
                WriteLiteral("<br>");
                WriteLiteral($"<input type='text' id='userForCreate' name='userForCreate' value='{_inputedServerConfiguration.UserForCreate}' style='margin: 0px 10px 10px; width:200px'>");
                WriteLiteral("<br>");
                WriteLiteral("<label for='passwordForCreate' style='padding: 4px 10px; color: #888; font-weight: bold;'>Password for creating schema: </label>");
                WriteLiteral("<br>");
                WriteLiteral($"<input type='text' id='passwordForCreate' name='passwordForCreate' value='{_inputedServerConfiguration.PasswordForCreate}' style='margin: 0px 10px 10px; width:200px'>");
                WriteLiteral("<br>");
                WriteLiteral("<button type='submit' style='padding: 1px 5px; font-size: 12px; line-height: 1.5; border-radius: 3px; color: #fff; background-color: #337ab7; border: 1px solid transparent; margin-left: 10px;'>");
                WriteLiteral("Create");
                WriteLiteral("</button>");
                if (_displayErrorMessage)
                {
                    WriteLiteral("&nbsp;&nbsp; <span style='color: red; font-weight: 600'>");
                    WriteLiteral(_errorMessage);
                    WriteLiteral("</span>");
                }
                WriteLiteral("</form>");
            }
        }

        private IEnumerable<ServerConfigurationViewModel> getConfigurations()
            => _configuration.BuildServerConfigurations().ToArray();
    }
}