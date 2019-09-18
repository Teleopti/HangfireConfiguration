namespace Hangfire.Configuration
{
    public class CreateServerConfiguration
    {
        public string StorageConnectionString { get; set; }
        public string SchemaCreatorConnectionString { get; set; }
        
        public string Server { get; set; }
        public string Database { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public string SchemaCreatorUser { get; set; }
        public string SchemaCreatorPassword { get; set; }
        
        public string SchemaName { get; set; }
    }
}