using CoachTimetableEditorApp.Authentication;
using CoachTimetableEditorApp.GoogleSheetlManager;
using Google.Apis.Drive.v3;
using Google.Apis.Sheets.v4;

namespace CoachTimetableEditorApp.GoogleDriveExcelManager
{
    public class GoogleSheetHandler : IGoogleSheetHandler
    {
        private readonly IGoogleAuthentication _googleAuthentication;

        public GoogleSheetHandler(IGoogleAuthentication googleAuthentication)
        {
            _googleAuthentication = googleAuthentication;
        }

        public void UpdateSheetCellValue()
        {
            var driveService = _googleAuthentication.GetDriveService();
            var sheetsService = _googleAuthentication.GetSheetsService();

            var fileList = GetFilesList(driveService);

            // Вывод информации о каждом файле
            if (fileList != null && fileList.Files.Count > 0)
            {
               
                foreach (var file in fileList.Files)
                {
                    
                   
                    var spreadsheetListRequest = sheetsService.Spreadsheets.Get(file.Id);
                    var spreadsheet = spreadsheetListRequest.Execute();
                    if (spreadsheet.Sheets != null && spreadsheet.Sheets.Count > 0)
                    {
                        Console.WriteLine("Список доступных таблиц:");
                        foreach (var sheet in spreadsheet.Sheets)
                        {
                            Console.WriteLine($"Название таблицы: {sheet.Properties.Title}");
                            Console.WriteLine($"ID таблицы: {sheet.Properties.SheetId}" +  "->" + file.Id);
                            Console.WriteLine();
                        }
                    }
                }
            }
        }
        static Google.Apis.Drive.v3.Data.FileList GetFilesList(DriveService driveService)
        {
           
            var request = driveService.Files.List();
            return request.Execute();

        }
    }
}
