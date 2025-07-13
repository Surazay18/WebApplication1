using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using WebApplication1.Models;
using MongoDB.Driver;
using System.Security.Claims;

namespace WebApplication1.Pages.Members
{
    [Authorize]
    public class CreateModel : PageModel
    {
        private readonly IMongoCollection<Member> _membersCollection;
        private readonly ILogger<CreateModel> _logger;

        public CreateModel(IMongoCollection<Member> membersCollection, ILogger<CreateModel> logger)
        {
            _membersCollection = membersCollection;
            _logger = logger;
        }

        [BindProperty]
        public Member Member { get; set; } = new Member();

        public void OnGet()
        {
            // Initialize default values if needed
            Member.JoinDate = DateTime.Now;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                // Set additional fields
                Member.ExpirationDate = Member.JoinDate.AddYears(1);
                Member.CreatedBy = User.FindFirstValue(ClaimTypes.NameIdentifier);
                Member.UpdatedBy = User.FindFirstValue(ClaimTypes.NameIdentifier);

                await _membersCollection.InsertOneAsync(Member);

                _logger.LogInformation("New member created: {Email}", Member.Email);
                return RedirectToPage("./Index", new { success = true });
            }
            catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
            {
                ModelState.AddModelError("Member.Email", "A member with this email already exists.");
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating member");
                ModelState.AddModelError(string.Empty, "An error occurred while saving the member.");
                return Page();
            }
        }
    }
}