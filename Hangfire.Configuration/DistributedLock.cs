using System;
using System.Data;
using System.Data.SqlClient;
using System.Transactions;


namespace Hangfire.Configuration
{
    public interface IDistributedLock
    {
        IDisposable TakeLock(TimeSpan takeLockTimeout);
    }

    public class DistributedLock : IDisposable, IDistributedLock
    {
        private readonly string _uniqueId;
        private readonly SqlConnection _sqlConnection;
        private bool _isLockTaken = false;
 
        public DistributedLock(
            string uniqueId,
            string connectionString)
        {
            _uniqueId = uniqueId;
            _sqlConnection = new SqlConnection(connectionString);
        }
 
        public IDisposable TakeLock(TimeSpan takeLockTimeout)
        {
            _sqlConnection.Open();
            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeOption.Suppress))
            {
                SqlCommand sqlCommand = new SqlCommand("sp_getapplock", _sqlConnection);
                sqlCommand.CommandType = CommandType.StoredProcedure;
                sqlCommand.CommandTimeout = (int)takeLockTimeout.TotalSeconds;
 
                sqlCommand.Parameters.AddWithValue("Resource", _uniqueId);
                sqlCommand.Parameters.AddWithValue("LockOwner", "Session");
                sqlCommand.Parameters.AddWithValue("LockMode", "Exclusive");
                sqlCommand.Parameters.AddWithValue("LockTimeout", (int)takeLockTimeout.TotalMilliseconds);
 
                SqlParameter returnValue = sqlCommand.Parameters.Add("ReturnValue", SqlDbType.Int);
                returnValue.Direction = ParameterDirection.ReturnValue;
                sqlCommand.ExecuteNonQuery();
 
                if ((int)returnValue.Value < 0)
                {
                    throw new Exception($"sp_getapplock failed with errorCode '{returnValue.Value}'");
                }
 
                _isLockTaken = true;
 
                transactionScope.Complete();
            }
 
            return this;
        }
 
        public void ReleaseLock()
        {
            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeOption.Suppress))
            {
                SqlCommand sqlCommand = new SqlCommand("sp_releaseapplock", _sqlConnection);
                sqlCommand.CommandType = CommandType.StoredProcedure;
 
                sqlCommand.Parameters.AddWithValue("Resource", _uniqueId);
                sqlCommand.Parameters.AddWithValue("LockOwner", "Session");
 
                sqlCommand.ExecuteNonQuery();
                _isLockTaken = false;
                transactionScope.Complete();
            }
        }
 
        public void Dispose()
        {
            if (_isLockTaken)
            {
                ReleaseLock();
            }
            _sqlConnection.Close();
        }
    }
}