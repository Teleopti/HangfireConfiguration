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
            .WaitAndRetry(10, i => TimeSpan.FromSeconds(Math.Min(30, Math.Pow(i, 2))));

        private static readonly Policy _commandRetry = Policy.Handle<TimeoutException>()
            .Or<SqlException>(DetectTransientSqlException.IsTransient)
            .OrInner<SqlException>(DetectTransientSqlException.IsTransient)
            .WaitAndRetry(10, i => TimeSpan.FromSeconds(i));

        public static void OpenWithRetry(this IDbConnection connection)
            => _connectionRetry.Execute(() => connection.Open());

        public static void ExecuteWithRetry(this IDbConnection connection, string sql, object param)
            => _commandRetry.Execute(() => connection.Execute(sql, param));
        
        public static void ExecuteWithRetry(this IDbConnection connection, string sql, object param, CommandType? commandType)
            => _commandRetry.Execute(() => connection.Execute(sql, param, commandType: commandType ));

        public static IEnumerable<T> QueryWithRetry<T>(this IDbConnection connection, string sql)
            => _commandRetry.Execute(() => connection.Query<T>(sql));
    }
}