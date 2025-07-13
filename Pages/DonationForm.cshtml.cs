using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApplication1.Models;

namespace WebApplication1.Pages
{
    public class DonationFormModel : PageModel
    {
        [BindProperty]
        public BloodDonationFormModel Donation { get; set; } = new();

        public string? SuccessMessage { get; set; }

        public void OnGet()
        {
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // TODO: Save Donation to database or process the data.
            SuccessMessage = "Thank you for your submission!";
            ModelState.Clear();
            Donation = new(); // Reset form

            return Page();
        }
    }
}
