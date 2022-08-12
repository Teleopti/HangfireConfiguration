namespace Hangfire.Configuration
{
    public class ViewModel
    {
        public int? Id { get; set; }
        public string Name { get; set; }
        public string ConnectionString { get; set; }
        public string SchemaName { get; set; }
        
        public bool Active { get; set; }
        public int? Workers { get; set; }
        public int? MaxWorkersPerServer { get; set; }
    }
}