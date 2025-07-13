using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApplication1.Pages
{
    public class ProfileModel : PageModel
    {
        // Properties to hold user profile data
        public string Username { get; set; }
        public string Email { get; set; }

        // Constructor
        public ProfileModel()
        {
            // Set some dummy data for now
            Username = "John Doe";
            Email = "johndoe@example.com";
        }

        public void OnGet()
        {
            // If needed, you can fetch user data from a database here
        }
    }
}
