using System;
using System.Collections.Generic;
using System.Linq;

namespace Hangfire.Configuration.Test.Domain.Fake
{
    public class FakeHangfireSchemaCreator : IHangfireSchemaCreator
    {
        public Exception TryConnectFailsWith;
        public IEnumerable<string> ConnectionTriedWith = Enumerable.Empty<string>();
        public IEnumerable<(string SchemaName, string ConnectionString)> Schemas = Enumerable.Empty<(string SchemaName, string ConnectionString)>();

        public void Has(string schemaName, string connectionString) =>
            Schemas = Schemas.Append((schemaName, connectionString)).ToArray();
        
        public void TryConnect(string connectionString)
        {
            ConnectionTriedWith = ConnectionTriedWith.Append(connectionString).ToArray();
            if (TryConnectFailsWith != null)
                throw TryConnectFailsWith;
        }

        public void CreateHangfireSchema(string schemaName, string connectionString)
        {
            Schemas = Schemas
                .Append((schemaName ?? ConnectionUtils.DefaultSchemaName(), connectionString))
                .ToArray();
        }

        public bool SchemaExists(string schemaName, string connectionString)
        {
            return Schemas
                .Where(x => string.Equals(x.SchemaName, schemaName, StringComparison.InvariantCultureIgnoreCase))
                .Where(x => x.ConnectionString == connectionString)
                .Any();
        }
    }
}