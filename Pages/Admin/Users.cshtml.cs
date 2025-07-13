// Pages/Admin/Users.cshtml.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Driver;
using WebApplication1.Models;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace WebApplication1.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class UsersModel : PageModel
    {
        private readonly IMongoCollection<User> _usersCollection;
        private readonly ILogger<UsersModel> _logger;

        public List<User> Users { get; set; } = new List<User>();

        public UsersModel(IMongoDatabase database, ILogger<UsersModel> logger)
        {
            _usersCollection = database.GetCollection<User>("Users");
            _logger = logger;
        }

        public async Task OnGetAsync()
        {
            try
            {
                Users = await _usersCollection.Find(_ => true).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading users");
                ModelState.AddModelError("", "Error loading user data");
            }
        }

        public async Task<IActionResult> OnPostPromoteAsync(string userId)
        {
            try
            {
                var filter = Builders<User>.Filter.Eq(u => u.Id, userId);
                var update = Builders<User>.Update.Set(u => u.Role, "Admin");
                await _usersCollection.UpdateOneAsync(filter, update);

                TempData["StatusMessage"] = "User promoted to admin successfully";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error promoting user {UserId}", userId);
                TempData["ErrorMessage"] = "Error promoting user";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDemoteAsync(string userId)
        {
            try
            {
                // Prevent demoting yourself
                if (userId == User.FindFirstValue(ClaimTypes.NameIdentifier))
                {
                    TempData["ErrorMessage"] = "You cannot demote yourself";
                    return RedirectToPage();
                }

                var filter = Builders<User>.Filter.Eq(u => u.Id, userId);
                var update = Builders<User>.Update.Set(u => u.Role, "User");
                await _usersCollection.UpdateOneAsync(filter, update);

                TempData["StatusMessage"] = "User demoted to regular user successfully";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error demoting user {UserId}", userId);
                TempData["ErrorMessage"] = "Error demoting user";
            }

            return RedirectToPage();
        }
    }
}
