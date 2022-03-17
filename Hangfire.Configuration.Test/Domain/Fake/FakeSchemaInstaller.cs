using System;
using System.Collections.Generic;
using System.Linq;

namespace Hangfire.Configuration.Test.Domain.Fake
{
    public class FakeSchemaInstaller : ISchemaInstaller
    {
        public Exception TryConnectFailsWith;
        public IEnumerable<string> ConnectionTriedWith = Enumerable.Empty<string>();
        
        public IEnumerable<(string SchemaName, string ConnectionString)> InstalledSchemas = 
	        Enumerable.Empty<(string SchemaName, string ConnectionString)>();

        public string InstalledHangfireConfigurationSchema;
        
        public void Has(string schemaName, string connectionString) =>
            InstalledSchemas = InstalledSchemas.Append((schemaName, connectionString)).ToArray();
        
        public void TryConnect(string connectionString)
        {
            ConnectionTriedWith = ConnectionTriedWith.Append(connectionString).ToArray();
            if (TryConnectFailsWith != null)
                throw TryConnectFailsWith;
        }

        public void InstallHangfireConfigurationSchema(string connectionString)
        {
	        InstalledHangfireConfigurationSchema = connectionString;
        }

        public void InstallHangfireStorageSchema(string schemaName, string connectionString)
        {
            InstalledSchemas = InstalledSchemas
                .Append((schemaName, connectionString))
                .ToArray();
        }

        public bool HangfireStorageSchemaExists(string schemaName, string connectionString)
        {
            return InstalledSchemas
                .Where(x => string.Equals(x.SchemaName, schemaName, StringComparison.InvariantCultureIgnoreCase))
                .Where(x => connectionString.StartsWith(x.ConnectionString))
                .Any();
        }
    }
}