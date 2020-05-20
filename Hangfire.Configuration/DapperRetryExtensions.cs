using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Polly;
using Dapper;

namespace Hangfire.Configuration
{
    internal static class DapperRetryExtensions
    {
        private static readonly Policy _connectionRetry = Policy.Handle<TimeoutException>()
            .Or<SqlException>(DetectTransientSqlException.IsTransient)
            .OrInner<SqlException>(DetectTransientSqlException.IsTransient)
            .WaitAndRetry(6, i => TimeSpan.FromSeconds(Math.Min(30, Math.Pow(i, 2))));

        private static readonly Policy _commandRetry = Policy.Handle<TimeoutException>()
            .Or<SqlException>(DetectTransientSqlException.IsTransient)
            .OrInner<SqlException>(DetectTransientSqlException.IsTransient)
            .WaitAndRetry(5, i => TimeSpan.FromSeconds(i));

        public static void OpenWithRetry(this IDbConnection connection)
            => _connectionRetry.Execute(() => connection.Open());
        
        public static IDbTransaction BeginTransactionWithRetry(this IDbConnection connection)
            => _connectionRetry.Execute(() => connection.BeginTransaction());
        
        public static void CommitWithRetry(this IDbTransaction transaction)
            => _connectionRetry.Execute(() => transaction.Commit());
        
        public static void ExecuteWithRetry(this IDbConnection connection, string sql, object param, IDbTransaction transaction = null)
            => _commandRetry.Execute(() => connection.Execute(sql, param, transaction));
        
        public static void ExecuteWithRetry(this IDbConnection connection, string sql, IDbTransaction transaction)
            => _commandRetry.Execute(() => connection.Execute(sql, null, transaction));
        
        public static IEnumerable<T> QueryWithRetry<T>(this IDbConnection connection, string sql, IDbTransaction transaction = null)
            => _commandRetry.Execute(() => connection.Query<T>(sql, null, transaction));
    }
}