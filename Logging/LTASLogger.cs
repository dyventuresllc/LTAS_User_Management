using LTAS_User_Management.Utilities;
using Relativity.API;
using System.Collections.Generic;
using System.Data.SqlClient;
using System;

namespace LTAS_User_Management.Logging
{
    public class LTASLogger : ILTASLogger
    {
        private readonly IDBContext _eddsDbContext;
        private readonly string _applicationName;
        private readonly string _source;
        private readonly LTASUMHelper _ltasHelper;

        public LTASLogger(IDBContext eddsDbContext, IHelper helper, IAPILog logger, string applicationName, string source)
        {
            _eddsDbContext = eddsDbContext ?? throw new ArgumentNullException(nameof(eddsDbContext));
            _applicationName = applicationName ?? throw new ArgumentNullException(nameof(applicationName));
            _source = source ?? throw new ArgumentNullException(nameof(source));
            _ltasHelper = new LTASUMHelper(helper, logger.ForContext<LTASLogger>());
        }

        public void LogInformation(string message)
        {
            Log("Information", message);
        }

        public void LogWarning(string message)
        {
            Log("Warning", message);
        }

        public void LogError(string message)
        {
            Log("Error", message);
        }

        public void LogError(Exception ex, string message)
        {
            Log("Error", message, ex);
        }

        public void LogDebug(string message)
        {
            Log("Debug", message);
        }

        private void Log(string level, string message, Exception exception = null)
        {
            try
            {
                string sql = @"
                    INSERT INTO QE.ApplicationLog_UserMgmt 
                    (LogDateTime, LogLevel, ApplicationName, Source, Message, 
                     ExceptionMessage, InnerException, StackTrace)
                    VALUES 
                    (@logDateTime, @logLevel, @applicationName, @source, @message,
                     @exceptionMessage, @innerException, @stackTrace)";

                var parameters = new List<SqlParameter>
                {
                    new SqlParameter("@logDateTime", DateTime.UtcNow),
                    new SqlParameter("@logLevel", level),
                    new SqlParameter("@applicationName", _applicationName),
                    new SqlParameter("@source", _source),
                    new SqlParameter("@message", message),
                    new SqlParameter("@exceptionMessage", (object)exception?.Message ?? DBNull.Value),
                    new SqlParameter("@innerException", (object)exception?.InnerException?.Message ?? DBNull.Value),
                    new SqlParameter("@stackTrace", (object)exception?.StackTrace ?? DBNull.Value)
                };

                _eddsDbContext.ExecuteNonQuerySQLStatement(sql, parameters);
            }
            catch (Exception ex)
            {
                // Use LTASUMHelper for logging errors
                _ltasHelper.Logger.LogError(ex, $"Failed to write to LTAS log: {ex.Message}");
            }
        }
    }
}