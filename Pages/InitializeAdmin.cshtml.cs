// Pages/InitializeAdmin.cshtml.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApplication1.Models;
using WebApplication1.Services;
using MongoDB.Driver;

namespace WebApplication1.Pages
{
    public class InitializeAdminModel : PageModel
    {
        private readonly IMongoCollection<User> _usersCollection;
        private readonly PasswordHasher _passwordHasher;

        public InitializeAdminModel(IMongoDatabase database, PasswordHasher passwordHasher)
        {
            _usersCollection = database.GetCollection<User>("Users");
            _passwordHasher = passwordHasher;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            // Check if admin already exists
            var adminExists = await _usersCollection.Find(u => u.Role == "Admin").AnyAsync();
            if (adminExists)
            {
                return Content("Admin already exists");
            }

            // Create admin credentials
            var username = "admin11";
            var password = "YourSecurePassword123!";

            // Hash the password
            var (hash, salt) = _passwordHasher.CreateHash(password);

            // Create admin user
            var adminUser = new User
            {
                Username = username,
                PasswordHash = hash,
                PasswordSalt = salt,
                Role = "Admin"
            };

            await _usersCollection.InsertOneAsync(adminUser);

            return Content("First admin user created successfully");
        }
    }
}