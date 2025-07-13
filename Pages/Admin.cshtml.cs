using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Driver;
using WebApplication1.Models;

namespace WebApplication1.Pages
{
    //[Authorize(Roles = "Admin")]  // Restrict access to Admins only
    //public class AdminModel : PageModel
    //{
    //    private readonly IMongoCollection<User> _usersCollection;

    //    public List<User> Users { get; set; } = new();

    //    public AdminModel(IMongoDatabase database)
    //    {
    //        _usersCollection = database.GetCollection<User>("Users");
    //    }

    //    // Load all users when the page loads
    //    public async Task OnGetAsync()
    //    {
    //        Users = await _usersCollection.Find(_ => true).ToListAsync();
    //    }

    //    // Promote a user to Admin
    //    public async Task<IActionResult> OnPostPromoteAsync(string userId)
    //    {
    //        var filter = Builders<User>.Filter.Eq(u => u.Id, userId);
    //        var update = Builders<User>.Update.Set(u => u.Role, "Admin");
    //        await _usersCollection.UpdateOneAsync(filter, update);
    //        return RedirectToPage();  // Refresh the page to show changes
    //    }

    //    // Demote an Admin to User
    //    public async Task<IActionResult> OnPostDemoteAsync(string userId)
    //    {
    //        var filter = Builders<User>.Filter.Eq(u => u.Id, userId);
    //        var update = Builders<User>.Update.Set(u => u.Role, "User");
    //        await _usersCollection.UpdateOneAsync(filter, update);
    //        return RedirectToPage();
    //    }
    //}

    // Pages/Admin/Index.cshtml.cs
    [Authorize(Roles = "Admin")]
    public class AdminModel : PageModel
    {
      
        private readonly IMongoCollection<User> _usersCollection;

        public AdminModel(IMongoDatabase database)
        {
            _usersCollection = database.GetCollection<User>("Users");
        }

        public long TotalUsers { get; set; }

        public void OnGet()
        {
            // Get total count of users
            TotalUsers = _usersCollection.CountDocuments(FilterDefinition<User>.Empty);

            ViewData["ActivePage"] = AdminNavPages.Index1;
        }

    }
}
