using System;

namespace LTAS_User_Management.Logging
{
    public interface ILTASLogger
    {
        void LogInformation(string message);
        void LogWarning(string message);
        void LogError(string message);
        void LogError(Exception ex, string message);
        void LogDebug(string message);
    }
}