namespace Hangfire.Configuration
{
    public class ConfigurationInfo
    {
        public int ConfigurationId { get; set; }
        public string Name { get; set; }
        public JobStorage JobStorage { get; set; }
    }
}