using kCura.Agent;
using Relativity.API;
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using LTAS_User_Management.Handlers;
using Relativity.Services.Objects;
using LTAS_User_Management.Logging;
using Relativity.Identity.V1.Services;
using kCura.Vendor.Castle.Core.Logging;

namespace LTAS_User_Management
{
    [kCura.Agent.CustomAttributes.Name("LTAS User Management")]    
    [Guid("596B6939-9E0C-476A-8B18-CC3EE92AAE48")]
    public class LTASUserManagementAgent: AgentBase
    {
        public override string Name => "LTAS User Management Worker";

        public override void Execute()
        {
            var relativityLogger = Helper.GetLoggerFactory().GetLogger().ForContext<LTASUserManagementAgent>();            
            var eddsDbContext = Helper.GetDBContext(-1);
            var ltasLogger = LoggerFactory.CreateLogger<LTASUserManagementAgent>(eddsDbContext, Helper, relativityLogger);

            //ltasLogger.LogInformation("Successfully connected to EDDS database");

            //string checkSQL = @"
            //        SELECT Message 
            //        FROM eddsdbo.Agent 
            //        WHERE Name LIKE 'LTAS Billing Management%'
            //        AND Message = 'Running'
            //        AND DATEDIFF(MINUTE, LastUpdate, GETDATE()) < 15";

            //var runningAgent = eddsDbContext.ExecuteSqlStatementAsScalar<string>(checkSQL);
            //ltasLogger.LogInformation($"Running agent check result: {runningAgent}");

            //if (runningAgent != null)
            //{
            //    ltasLogger.LogInformation("Another LTAS agent is currently running. Exiting.");
            //    return;
            //}
            
            //ltasLogger.LogInformation("No running agents found, continuing execution");
            RaiseMessage("Starting LTAS User Management...", 10);

            try
            {
                var instanceSettingBundle = Helper.GetInstanceSettingBundle();
                var jobHandler = new JobHandler(relativityLogger, eddsDbContext, Helper);
                var dataHandler = new DataHandler(eddsDbContext, Helper, relativityLogger);

                using (var objectManager = Helper.GetServicesManager().CreateProxy<IObjectManager>(ExecutionIdentity.System))
                {
                    if (jobHandler.ShouldExecuteJob(eddsDbContext))
                    {
                        ltasLogger.LogInformation("Job execution criteria met, starting process...");
                        relativityLogger.LogInformation("Job execution criteria met, starting process...");

                        ProcessUserJobsAsync(
                            dataHandler,                            
                            instanceSettingBundle,
                            relativityLogger,
                            eddsDbContext).GetAwaiter().GetResult();

                        jobHandler.UpdateJobExecutionTime(eddsDbContext);

                        ltasLogger.LogInformation("Job execution completed successfully");
                        relativityLogger.LogInformation("Job execution completed successfully");
                    }
                    else
                    {
                        ltasLogger.LogInformation("Job execution criteria not met, skipping process");
                        relativityLogger.LogInformation("Job execution criteria not met, skipping process");
                    }
                }
            }
            catch (Exception ex)
            {
                ltasLogger.LogError(ex, "Error executing user management");
                relativityLogger.LogError(ex, "Error executing user management");
                RaiseMessage("Error executing user management", 1);
            }
        }

        private async Task ProcessUserJobsAsync(
        DataHandler dataHandler,        
        IInstanceSettingsBundle instanceSettingsBundle,
        IAPILog logger,
        IDBContext eddsDbContext)
        {
            using (var userManager = Helper.GetServicesManager()
                .CreateProxy<IUserManager>(ExecutionIdentity.System))
            {
                var manager = new UserManager(                    
                    dataHandler,
                    logger,
                    Helper,
                    instanceSettingsBundle,
                    eddsDbContext,
                    userManager);

                await Task.Run(async () =>
                {
                    await manager.ProcessUserManagementRoutinesAsync();
                }).ConfigureAwait(false);
            }
        }
               
        public class JobHandler
        {
            private readonly IAPILog _logger;
            private readonly ILTASLogger _ltasLogger;

            public JobHandler(IAPILog logger, IDBContext eddsDbContext, IHelper helper)
            {
                _logger = logger;
                _ltasLogger = LoggerFactory.CreateLogger<JobHandler>(eddsDbContext, helper, logger);
            }

            public bool ShouldExecuteJob(IDBContext eddsDbContext)
            {
                try
                {              
                    string jobSQL = @"
                        SELECT 
                            JobExecute_Interval,
                            JobLastExecute_DateTime,
                            JobLastCheck_DateTime
                        FROM EDDS.QE.AutomationControl 
                        WHERE JobId = 5";

                    var jobInfo = eddsDbContext.ExecuteSqlStatementAsDataTable(jobSQL).Rows[0];

                    // Update the last check time
                    eddsDbContext.ExecuteNonQuerySQLStatement(@"
                        UPDATE qac 
                        SET qac.JobLastCheck_DateTime = GETDATE()
                        FROM EDDS.QE.AutomationControl qac 
                        WHERE qac.JobId = 5");
                    
                    int intervalHours = Convert.ToInt32(jobInfo["JobExecute_Interval"]);
                    DateTime lastExecuteTime = jobInfo["JobLastExecute_DateTime"] != DBNull.Value
                        ? Convert.ToDateTime(jobInfo["JobLastExecute_DateTime"])
                        : DateTime.MinValue;
                    
                    var now = DateTime.Now;

                    TimeSpan timeSinceLastExecution = now - lastExecuteTime;
                    bool shouldExecute = timeSinceLastExecution.TotalHours >= intervalHours;

                    _ltasLogger.LogInformation($"Job check - Interval: {intervalHours}h, Last execution: {lastExecuteTime}, Should execute: {shouldExecute}");
                    _logger.LogInformation($"Job check - Interval: {intervalHours}h, Last execution: {lastExecuteTime}, Should execute: {shouldExecute}");

                    return shouldExecute;
                }
                catch (Exception ex)
                {
                    _ltasLogger.LogError(ex, "Error checking job execution status");
                    _logger.LogError(ex, "Error checking job execution status");
                    return false;
                }
            }

            public void UpdateJobExecutionTime(IDBContext eddsDbContext)
            {
                try
                {
                    eddsDbContext.ExecuteNonQuerySQLStatement(
                        @"UPDATE qac 
                        SET qac.[JobLastExecute_DateTime] = GETDATE()
                        FROM EDDS.QE.AutomationControl qac 
                        WHERE qac.JobId = 5");

                    _ltasLogger.LogInformation("Successfully updated job execution time");
                    _logger.LogInformation("Successfully updated job execution time");
                }
                catch (Exception ex)
                {
                    _ltasLogger.LogError(ex, "Failed to update job execution time");
                    _logger.LogError(ex, "Failed to update job execution time");
                    throw;
                }
            }
        }
    }
}
