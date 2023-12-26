using CoachTimetableEditorApp.Authentication;
using CoachTimetableEditorApp.ExceptionHandling;
using CoachTimetableEditorApp.GoogleDriveExcelManager;
using CoachTimetableEditorApp.GoogleSheetlManager;
using CoachTimetableEditorApp.QuatzManager;
using Quartz;
using Serilog;

namespace CoachTimetableEditorApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var logFilePath = Path.Combine("Logs", "log.txt");

            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddScoped<IGoogleAuthentication, GoogleAuthentication>();
            builder.Services.AddScoped<IGoogleSheetHandler, GoogleSheetHandler>();
            builder.Logging.AddSerilog();

            Log.Logger = new LoggerConfiguration()
                .WriteTo.File(logFilePath, rollingInterval: RollingInterval.Month)
                .CreateLogger();

            builder.Services.AddQuartz(configure =>
            {
                var jobKey = new JobKey(nameof(UpdateGoogleSheetJob));
                configure
                    .AddJob<UpdateGoogleSheetJob>(jobKey)
                    .AddTrigger(trigger => trigger.ForJob(jobKey).WithCronSchedule("0 0 0 1 * ? *"))
                    .AddJobListener<JobListener>();
            });

            builder.Services.AddQuartzHostedService(options =>
            {
                options.WaitForJobsToComplete = true;
            });

            var app = builder.Build();

            app.Run();
        }
    }
}
