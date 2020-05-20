using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Polly;
using Dapper;

namespace Hangfire.Configuration
{
    public interface IUnitOfWork
    {
        void Execute(string sql);
        void Execute(string sql, object param);
        IEnumerable<T> Query<T>(string sql);
    }

    public abstract class UnitOfWorkBase : IUnitOfWork
    {
        protected abstract void operation(Action<IDbConnection, IDbTransaction> action);

        private static readonly Policy _connectionRetry = Policy.Handle<TimeoutException>()
            .Or<SqlException>(DetectTransientSqlException.IsTransient)
            .OrInner<SqlException>(DetectTransientSqlException.IsTransient)
            .WaitAndRetry(6, i => TimeSpan.FromSeconds(Math.Min(30, Math.Pow(i, 2))));
        
        public void Execute(string sql)
        {
            operation((c, t) => { c.Execute(sql, null, t); });
        }

        public void Execute(string sql, object param)
        {
            operation((c, t) => { c.Execute(sql, param, t); });
        }

        public IEnumerable<T> Query<T>(string sql)
        {
            var result = default(IEnumerable<T>);
            operation((c, t) => { result = c.Query<T>(sql, null, t); });
            return result;
        }

        protected void OpenWithRetry(IDbConnection connection)
        {
            _connectionRetry.Execute(() => connection.Open());
        }
    }

    public class UnitOfWork : UnitOfWorkBase
    {
        public string ConnectionString { get; set; }

        protected override void operation(Action<IDbConnection, IDbTransaction> action)
        {
            using (var conn = new SqlConnection(ConnectionString))
            {
                OpenWithRetry(conn);
                action.Invoke(conn, null);
            }
        }
    }

    public class UnitOfWorkTransaction : UnitOfWorkBase, IDisposable
    {
        private readonly IDbConnection _connection;
        private readonly IDbTransaction _transaction;

        public UnitOfWorkTransaction(string connectionString)
        {
            _connection = new SqlConnection(connectionString);
            OpenWithRetry(_connection);
            _transaction = _connection.BeginTransaction();
        }

        protected override void operation(Action<IDbConnection, IDbTransaction> action)
        {
            action.Invoke(_connection, _transaction);
        }

        public void Commit()
        {
            _transaction.Commit();
        }

        public void Dispose()
        {
            _transaction.Dispose();
            _connection.Dispose();
        }
    }
}