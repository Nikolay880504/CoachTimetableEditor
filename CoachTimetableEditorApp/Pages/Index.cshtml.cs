using CoachTimetableEditorApp.Authentication;
using CoachTimetableEditorApp.GoogleSheetlManager;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Cryptography.X509Certificates;

namespace CoachTimetableEditorApp.Pages
{
    public class IndexModel : PageModel
    {

        private readonly IGoogleSheetHandler _googleSheetHandler;
   
        public IndexModel(IGoogleSheetHandler googleSheetHandler)
        {
          _googleSheetHandler = googleSheetHandler;          
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        public void OnPostConfigureDriveService()
        {
            _googleSheetHandler.UpdateSheetCellValueAsync();
        }
    }
}