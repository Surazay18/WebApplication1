using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Driver;
using WebApplication1.Models;
using Microsoft.AspNetCore.Authorization;

namespace WebApplication1.Pages.Members
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly IMongoCollection<Member> _membersCollection;

        public IndexModel(IMongoCollection<Member> membersCollection)
        {
            _membersCollection = membersCollection;
        }

        public List<Member> Members { get; set; } = new List<Member>();
        public bool ShowSuccessMessage { get; set; }
        

       

        public async Task OnGetAsync(bool success = false)
        {
            ShowSuccessMessage = success;
            Members = await _membersCollection.Find(_ => true)
                                           .SortByDescending(m => m.JoinDate)
                                           .Limit(100)
                                           .ToListAsync();
        }
    }
}