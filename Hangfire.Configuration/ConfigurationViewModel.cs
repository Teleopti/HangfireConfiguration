namespace Hangfire.Configuration
{
    public class ConfigurationViewModel
    {
        public int? Id { get; set; }
        public string ServerName { get; set; }
        public string DatabaseName { get; set; }
        public string SchemaName { get; set; }
        public string Active { get; set; }
    }
}