using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using MongoDB.Driver;
using WebApplication1.Models;

namespace WebApplication1.Pages
{
    [Authorize]
    public class SecurePageModel : PageModel
    {
        private readonly IMongoCollection<User> _usersCollection;

        public SecurePageModel(IMongoDatabase database)
        {
            _usersCollection = database.GetCollection<User>("Users");
        }

        public long TotalUsers { get; set; }

        public void OnGet()
        {
            // Get total count of users
            TotalUsers = _usersCollection.CountDocuments(FilterDefinition<User>.Empty);
        }
    }
}
