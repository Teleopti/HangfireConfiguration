namespace Hangfire.Configuration
{
    public class StoredConfiguration
    {
        public int? Id { get; set; }
        public string ConnectionString { get; set; }
        public string SchemaName { get; set; }
        public int? Workers { get; set; }
        public bool? Active { get; set; }
    }
}