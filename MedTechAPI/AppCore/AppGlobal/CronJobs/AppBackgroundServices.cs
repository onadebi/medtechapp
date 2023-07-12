using Hangfire;

namespace MedTechAPI.AppCore.AppGlobal.CronJobs
{
    public class AppBackgroundServices
    {
        public static void RecurringJobsSchedule()
        {
            //RecurringJob.RemoveIfExists(nameof(BankRepoService.PostTransaction));
            //RecurringJob.AddOrUpdate<IBankRepoService>(m => m.PostTransaction(), "*/2 * * * *");
        }
    }
}
