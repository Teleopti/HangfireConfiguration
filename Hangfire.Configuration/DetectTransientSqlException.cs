using System.Data.SqlClient;
using System.Linq;

namespace Hangfire.Configuration
{
    internal static class DetectTransientSqlException
    {
        public static bool IsTransient(SqlException ex)
        {
            if (!innerIsTransient(ex))
                return isTransientTimeout(ex);

            return true;
        }

        private static bool innerIsTransient(SqlException ex)
        {
            foreach (SqlError error in ex.Errors)
            {
                switch (error.Number)
                {
                    case -2: //Timeout!
                    case 20:
                    case 64:
                    case 233:
                    case 4060:
                    case 10053:
                    case 10054:
                    case 10060:
                    case 10928:
                    case 10929:
                    case 40143:
                    case 40197:
                    case 40540:
                    case 40613:
                    case 40501:
                    case 40648:
                    case 40671:
                    case 42019:
                    case 45168:
                    case 45169:
                    case 49918:
                    case 49919:
                    case 49920:
                        return true;
                    default:
                        continue;
                }
            }

            return false;
        }

        private static bool isTransientTimeout(SqlException ex)
        {
            if (isConnectionTimeout(ex))
                return true;

            return false;
        }

        private static bool isConnectionTimeout(SqlException ex)
        {
            if (ex.Errors.Cast<SqlError>().Any(error => error.Number == -2 || error.Number == 121))
                return true;

            if (isNetworkTimeout(ex))
                return true;

            return isForciblyClosedExistingConnection(ex);
        }

        private static bool isNetworkTimeout(SqlException exception)
        {
            return exception.Errors.Cast<SqlError>().Any(error => error.Number == 40);
        }

        private static bool isForciblyClosedExistingConnection(SqlException exception)
        {
            return exception.Errors.Cast<SqlError>().Any(error => error.Message.Contains("existing connection was forcibly closed") || error.Message.Contains("marked by the server as unrecoverable"));
        }
    }
}