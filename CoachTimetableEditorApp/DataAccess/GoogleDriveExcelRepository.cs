using CoachTimetableEditorApp.Authentication;
using Google.Apis.Drive.v3;

namespace CoachTimetableEditorApp.DataAccess
{
    public class GoogleDriveExcelRepository
    {
        /*
        private readonly DriveService _service;
        public GoogleDriveExcelRepository(IGoogleDriveAuthentication googleDriveAuthentication)
        {
            _service = googleDriveAuthentication.GetConfiguredDriveService();
        }
        public async Task<List<byte[]>> GetExelFileFromDriveAsync()
        {
            var result = new List<byte[]>();

            var fileListRequest = _service.Files.List();
            var fileList = await fileListRequest.ExecuteAsync();

            foreach (var file in fileList.Files)
            {
                var fileId = file.Id;
                var fileRequest = _service.Files.Get(fileId);
                var steram = new MemoryStream();

                using (var stream = new MemoryStream())
                {
                     fileRequest.Download(stream);
                    byte[] fileContent = stream.ToArray();
                }
               
            }
            //service
            return result;
        }
        */
    }
}
