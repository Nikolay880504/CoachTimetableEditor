using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;

namespace CoachTimetableEditorApp.DataAccess.Authentication
{
    public class GoogleDriveAuthentication
    {

        private static DriveService GetService()
        {
            var applicationName = "CoachTimetableEditorApp";
           // var userName = "coach-timetable-editor-app-ser@coachtimetableeditorapp.iam.gserviceaccount.com";

            var credential = GoogleCredential.FromFile("path/to/your/json/file.json")
                                              .CreateScoped(DriveService.Scope.Drive)
                                              .UnderlyingCredential as ServiceAccountCredential;

            var service = new DriveService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = applicationName
            });

            return service;
        }
    }
}
