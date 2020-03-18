using System;
using System.Data;
using System.Data.SqlClient;
using System.Runtime.ExceptionServices;
using Dapper;

namespace Hangfire.Configuration
{
    public interface IDistributedLock
    {
        IDisposable Take(string resource);
    }

    public class DistributedLock : IDistributedLock
    {
        private readonly string _connectionString;

        public DistributedLock(string connectionString)
        {
            _connectionString = connectionString;
        }

        public IDisposable Take(string resource)
        {
            var sqlConnection = new SqlConnection(_connectionString);
            sqlConnection.OpenWithRetry();

            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("@Resource", resource);
                parameters.Add("@LockMode", "Exclusive");
                parameters.Add("@LockOwner", "Session");
                parameters.Add("@LockTimeout", (int) TimeSpan.FromSeconds(10).TotalMilliseconds);
                parameters.Add("@Result", dbType: DbType.Int32, direction: ParameterDirection.ReturnValue);

                sqlConnection.ExecuteWithRetry(@"sp_getapplock", parameters, CommandType.StoredProcedure);

                var lockResult = parameters.Get<int>("@Result");

                if (lockResult < 0)
                    throw new Exception($"sp_getapplock failed with errorCode '{lockResult}'");
            }
            catch (Exception ex)
            {
                sqlConnection.Close();
                ExceptionDispatchInfo.Capture(ex).Throw();
            }

            return new GenericDisposable(() =>
            {
                var parameters = new DynamicParameters();
                parameters.Add("@Resource", resource);
                parameters.Add("@LockOwner", "Session");

                sqlConnection.ExecuteWithRetry(@"sp_releaseapplock", parameters, CommandType.StoredProcedure);

                sqlConnection.Close();
            });
        }
    }
}