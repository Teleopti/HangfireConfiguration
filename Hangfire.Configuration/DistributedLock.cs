using System;
using System.Data;
using System.Data.SqlClient;
using System.Transactions;


namespace Hangfire.Configuration
{
    public interface IDistributedLock
    {
        IDisposable Take(TimeSpan lockTimeout);
    }

    public class DistributedLock : IDisposable, IDistributedLock
    {
        private readonly string _uniqueId;
        private readonly SqlConnection _sqlConnection;
        private bool _isLockTaken;
 
        public DistributedLock(
            string uniqueId,
            string connectionString)
        {
            _uniqueId = uniqueId;
            _sqlConnection = new SqlConnection(connectionString);
        }
 
        public IDisposable Take(TimeSpan lockTimeout)
        {
            _sqlConnection.Open();
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Suppress))
            {
                SqlCommand command = new SqlCommand("sp_getapplock", _sqlConnection);
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = (int)lockTimeout.TotalSeconds;
 
                command.Parameters.AddWithValue("Resource", _uniqueId);
                command.Parameters.AddWithValue("LockOwner", "Session");
                command.Parameters.AddWithValue("LockMode", "Exclusive");
                command.Parameters.AddWithValue("LockTimeout", (int)lockTimeout.TotalMilliseconds);
 
                SqlParameter returnValue = command.Parameters.Add("ReturnValue", SqlDbType.Int);
                returnValue.Direction = ParameterDirection.ReturnValue;
                command.ExecuteNonQuery();
 
                if ((int)returnValue.Value < 0)
                {
                    throw new Exception($"sp_getapplock failed with errorCode '{returnValue.Value}'");
                }
 
                _isLockTaken = true;
 
                scope.Complete();
            }
 
            return this;
        }
 
        public void ReleaseLock()
        {
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Suppress))
            {
                SqlCommand command = new SqlCommand("sp_releaseapplock", _sqlConnection);
                command.CommandType = CommandType.StoredProcedure;
 
                command.Parameters.AddWithValue("Resource", _uniqueId);
                command.Parameters.AddWithValue("LockOwner", "Session");
 
                command.ExecuteNonQuery();
                _isLockTaken = false;
                scope.Complete();
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