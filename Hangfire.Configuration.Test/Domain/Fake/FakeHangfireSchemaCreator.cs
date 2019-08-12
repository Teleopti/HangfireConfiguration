using System;
using System.Collections.Generic;
using System.Linq;

namespace Hangfire.Configuration.Test.Domain.Fake
{
    public class FakeHangfireSchemaCreator : IHangfireSchemaCreator
    {
        public Exception TryConnectFailsWith;
        public IEnumerable<string> ConnectionTriedWith = Enumerable.Empty<string>();
        public IEnumerable<(string SchemaName, string ConnectionString)> SchemaCreatedWith = Enumerable.Empty<(string SchemaName, string ConnectionString)>();

        public void TryConnect(string connectionString)
        {
            ConnectionTriedWith = ConnectionTriedWith.Append(connectionString).ToArray();
            if (TryConnectFailsWith != null)
                throw TryConnectFailsWith;
        }

        public void CreateHangfireSchema(string schemaName, string connectionString)
        {
            SchemaCreatedWith = SchemaCreatedWith.Append((schemaName, connectionString)).ToArray();
        }
    }
}