using CoachTimetableEditorApp.Authentication;
using CoachTimetableEditorApp.GoogleSheetlManager;
using Google.Apis.Drive.v3;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using System.Globalization;
using static Google.Apis.Sheets.v4.SpreadsheetsResource.ValuesResource;

namespace CoachTimetableEditorApp.GoogleDriveExcelManager
{
    public class GoogleSheetHandler : IGoogleSheetHandler
    {
        private readonly IGoogleAuthentication _googleAuthentication;
        private readonly ILogger<GoogleSheetHandler> _logger;

        private DriveService _driveService;
        private readonly SheetsService _sheetsService;
        private DateTime _currentDateTime;
        private CultureInfo _cultureInfo;

        private const string ClearSheetRangeString = "!C4:AG56";
        private const string MonthNamesRangeString = "!1:1";
        private const string DayOfWeekRangeString = "!C2:AG2";
        private const string NumbersDayOfMonthRangeString = "!C3:AG3";

        public GoogleSheetHandler(IGoogleAuthentication googleAuthentication, ILogger<GoogleSheetHandler> logger)
        {
            _googleAuthentication = googleAuthentication;
            _logger = logger;
            _driveService = _googleAuthentication.GetDriveService();
            _sheetsService = _googleAuthentication.GetSheetsService();

        }

        public void UpdateSheetCellValue()
        {
            var fileList = GetFilesList(_driveService);
            SetCurrentDate();
            SetCultureInfoForUkraine();

            if (fileList != null && fileList.Files.Count > 0)
            {
                foreach (var file in fileList.Files)
                {
                    var spreadsheetListRequest = _sheetsService.Spreadsheets.Get(file.Id);
                    var spreadsheet = spreadsheetListRequest.Execute();

                    UpdateFileNameToCurrentMonth(spreadsheet.SpreadsheetId, file.Name);

                    if (spreadsheet.Sheets != null && spreadsheet.Sheets.Count > 0)
                    {
                        foreach (var sheet in spreadsheet.Sheets)
                        {
                            if (sheet.Properties != null)
                            {
                                ClearSheetRange(spreadsheet.SpreadsheetId, sheet.Properties.Title);
                                UpdateMonthNames(spreadsheet.SpreadsheetId, sheet.Properties.Title);
                                MapDatesToDaysOfWeek(spreadsheet.SpreadsheetId, sheet.Properties.Title);
                            }
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
        private void ClearSheetRange(string spreadsheetId, string nameSheet)
        {
             
            if (spreadsheetId == null || string.IsNullOrEmpty(nameSheet))
            {
                return;
            }
            var range = $"{nameSheet}{ClearSheetRangeString}";
            var clearRequestBody = new ClearValuesRequest();

            var clearRequest = _sheetsService.Spreadsheets.Values.Clear(clearRequestBody, spreadsheetId, range);

            clearRequest.Execute();
        }
        private void UpdateMonthNames(string spreadsheetId, string nameSheet)
        {
            var curentMonthAndYear = $"{GetCurrentYear()} {_currentDateTime.ToString("MMMM", _cultureInfo).ToUpper()}";
            var range = $"{nameSheet}{MonthNamesRangeString}";

            var valueRange = new ValueRange();
            valueRange.Values = new List<IList<object>>() { new List<object> { curentMonthAndYear } };

            var updateRequest = _sheetsService.Spreadsheets.Values.Update(valueRange, spreadsheetId, range);
            updateRequest.ValueInputOption = UpdateRequest.ValueInputOptionEnum.RAW;

            updateRequest.Execute();
        }
        private void MapDatesToDaysOfWeek(string spreadsheetId, string nameSheet)
        {
            var rangeForDayOfWeek = $"{nameSheet}{DayOfWeekRangeString}";
            var rangeNumbersDayOfMonth = $"{nameSheet}{NumbersDayOfMonthRangeString}";
            var calendar = _cultureInfo.Calendar;
            var daysInMonth = calendar.GetDaysInMonth(GetCurrentYear(), GetCurrentMonth());

            var listOfNumbersDayOfMonth = new List<object>();
            var listOfDayOfWeek = new List<object>();

            for (int day = 1; day <= 31; day++)
            {
                if (day > daysInMonth)
                {
                    listOfNumbersDayOfMonth.Add(string.Empty);
                    listOfDayOfWeek.Add(string.Empty);
                }
                else
                {
                    listOfNumbersDayOfMonth.Add(day);
                    var specifiedDate = new DateTime(GetCurrentYear(), GetCurrentMonth(), day);
                    listOfDayOfWeek.Add(specifiedDate.ToString("ddd", _cultureInfo));
                }
            }

            var valueRangeForDayOfWeek = new ValueRange { Values = new List<IList<object>> { listOfDayOfWeek } };
            var valueRangeForNumbers = new ValueRange { Values = new List<IList<object>> { listOfNumbersDayOfMonth } };

            var updateRequestForDayOfWeek = _sheetsService.Spreadsheets.Values.Update(valueRangeForDayOfWeek, spreadsheetId, rangeForDayOfWeek);
            updateRequestForDayOfWeek.ValueInputOption = UpdateRequest.ValueInputOptionEnum.RAW;

            var updateRequestForNumbers = _sheetsService.Spreadsheets.Values.Update(valueRangeForNumbers, spreadsheetId, rangeNumbersDayOfMonth);
            updateRequestForNumbers.ValueInputOption = UpdateRequest.ValueInputOptionEnum.RAW;

            updateRequestForDayOfWeek.Execute();
            updateRequestForNumbers.Execute();

        }
        private void UpdateFileNameToCurrentMonth(string spreadsheetId, string fileName)
        {
            var ukranianMonth = new string[] { "СІЧЕНЬ","ЛЮТИЙ","БЕРЕЗЕНЬ","КВІТЕНЬ","ТРАВЕНЬ","ЛИПЕНЬ","СІЧЕНЬ","СЕРПЕНЬ",
            "ВЕРЕСЕНЬ","ЖОВТЕНЬ","ЛИСТОПАД","ГРУДЕНЬ"
        };
            var newFileNameParts = new List<string>();
            var splitFileName = fileName.Split(' ');
            foreach (var item in splitFileName)
            {
                if (ukranianMonth.Contains(item))
                {
                    newFileNameParts.Add(_currentDateTime.ToString("MMMM", _cultureInfo).ToUpper());
                }
                else if (int.TryParse(item, out _))
                {
                    newFileNameParts.Add(GetCurrentYear().ToString());
                }
                else
                {
                    newFileNameParts.Add(item);
                }
            }
            var newFileName = string.Join(" ", newFileNameParts);

            CreatingRequestToChangeTheFileName(newFileName, spreadsheetId);
        }
        private void CreatingRequestToChangeTheFileName(string newFileName, string spreadsheetId)
        {
            var request = new BatchUpdateSpreadsheetRequest
            {
                Requests = new List<Request>
        {
            new Request
            {
                UpdateSpreadsheetProperties = new UpdateSpreadsheetPropertiesRequest
                {
                    Properties = new SpreadsheetProperties
                    {
                        Title = newFileName
                    },
                    Fields = "title"
                }
            }
        }
            };
            _sheetsService.Spreadsheets.BatchUpdate(request, spreadsheetId).Execute();
        }
        private void SetCurrentDate()
        {
            _currentDateTime = DateTime.Now.Date;
        }
        private int GetCurrentMonth()
        {
            return _currentDateTime.Month;
        }

        private int GetCurrentYear()
        {
            return _currentDateTime.Year % 100;
        }

        private void SetCultureInfoForUkraine()
        {
            _cultureInfo = new CultureInfo("uk-UA");
        }
    }
}
