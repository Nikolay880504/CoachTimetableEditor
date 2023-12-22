using CoachTimetableEditorApp.Authentication;
using CoachTimetableEditorApp.GoogleDriveExcelManager;
using CoachTimetableEditorApp.GoogleSheetlManager;
using CoachTimetableEditorApp.QuatzManager;
using Quartz;

namespace CoachTimetableEditorApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorPages();

            builder.Services.AddScoped<IGoogleAuthentication, GoogleAuthentication>();
            builder.Services.AddScoped<IGoogleSheetHandler, GoogleSheetHandler>();

            builder.Services.AddQuartz(configure =>
            {
                var jobKey = new JobKey(nameof(UpdateGoogleSheetJob));
                configure
                    .AddJob<UpdateGoogleSheetJob>(jobKey)
                    .AddTrigger(
                        trigger => trigger.ForJob(jobKey).WithCronSchedule("0 0 0 1 * ? *"));

              //  configure.UseMicrosoftDependencyInjectionJobFactory();
            });

            builder.Services.AddQuartzHostedService(options =>
            {
                options.WaitForJobsToComplete = true;
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapRazorPages();

            app.Run();
        }

    }
}