namespace Hangfire.Configuration
{
    public interface IHangfireSchemaCreator
    {
        void TryConnect(string connectionString);
        void CreateHangfireSchema(string schemaName, string connectionStringForCreate);
    }
}