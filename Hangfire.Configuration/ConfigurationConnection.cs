using System;
using System.Data;
using System.Data.SqlClient;

namespace Hangfire.Configuration
{
    public interface IConfigurationConnection
    {
        void UseConnection(Action<IDbConnection> action);
        IDbTransaction Transaction();
    }

    public class ConfigurationConnection : IConfigurationConnection
    {
        public string ConnectionString { get; set; }

        public void UseConnection(Action<IDbConnection> action)
        {
            using (var conn = new SqlConnection(ConnectionString))
            {
                conn.OpenWithRetry();
                action.Invoke(conn);
            }
        }

        public IDbTransaction Transaction() => 
            null;
    }    
    
    public class ConfigurationConnectionTransaction : IConfigurationConnection, IDisposable
    {
        private readonly IDbConnection _conn;
        private readonly IDbTransaction _transaction;

        public ConfigurationConnectionTransaction(string connectionString)
        {
            _conn = new SqlConnection(connectionString);
            _conn.OpenWithRetry();
            _transaction = _conn.BeginTransactionWithRetry();
        }

        public void UseConnection(Action<IDbConnection> action)
        {
            action.Invoke(_conn);
        }

        public IDbTransaction Transaction() => 
            _transaction;

        public void Commit()
        {
            _transaction.CommitWithRetry();
        }
        
        public void Dispose()
        {
            _transaction.Dispose();
            _conn.Dispose();
        }
    }
}