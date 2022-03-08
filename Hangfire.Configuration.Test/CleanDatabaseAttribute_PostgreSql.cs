namespace Hangfire.Configuration.Test
{
    public class CleanDatabaseAttribute : CleanDatabasePostgresAttribute
    {
	    public CleanDatabaseAttribute()
	    {
	    }
	    
	    public CleanDatabaseAttribute(int schemaVersion) : base(schemaVersion)
	    {
	    }
    }
}