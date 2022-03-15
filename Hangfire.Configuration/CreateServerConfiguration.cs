using Hangfire.Configuration.Internals;

namespace Hangfire.Configuration
{
    public class CreateServerConfiguration
    {
        public string Name { get; set; }
        
        public string Server { get; set; }
        public string Database { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public string SchemaCreatorUser { get; set; }
        public string SchemaCreatorPassword { get; set; }
        
        public string SchemaName { get; set; }
        public string DatabaseProvider { get; set; }

        internal ICreateServerConfiguration CreateCreator(IConfigurationStorage storage, IHangfireSchemaCreator creator)
        {
	        if (DatabaseProvider == "PostgreSql")
		        return new PostgresCreateServerConfiguration(storage, creator);
	        if (DatabaseProvider == "redis")
		        return new RedisCreateServerConfiguration(storage);
	        return new SqlServerCreateServerConfiguration(storage, creator);
        }
    }
}