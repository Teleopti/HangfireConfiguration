using System.Collections.Generic;

namespace Hangfire.Configuration
{
    public class ServerConfigurationViewModel
    {
        public int? Id { get; set; }
        public string ServerName { get; set; }
        public string DatabaseName { get; set; }
        public string SchemaName { get; set; }
        public string Active { get; set; }
        public int? Workers { get; set; }
        public bool IsDefault { get; set; }
        public string Title { get; set; } = "Default Hangfire Server";
    }
}