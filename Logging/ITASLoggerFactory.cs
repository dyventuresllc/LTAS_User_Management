using Relativity.API;

namespace LTAS_User_Management.Logging
{
    public static class LoggerFactory
    {
        public static ILTASLogger CreateLogger<T>(
            IDBContext eddsDbContext,
            IHelper helper,
            IAPILog logger,
            string applicationName = "LTAS User Management")
        {
            return new LTASLogger(
                eddsDbContext,
                helper,
                logger,
                applicationName,
                typeof(T).Name
            );
        }
    }
}