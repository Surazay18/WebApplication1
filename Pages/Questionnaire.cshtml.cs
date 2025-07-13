// Pages/Questionnaire.cshtml.cs
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Driver;

public class QuestionnaireModel : PageModel
{
    private readonly IMongoCollection<Questionnaire> _questionnaires;
    private readonly IMongoCollection<QuestionnaireResponse> _responses;

    public Questionnaire? ActiveQuestionnaire { get; set; }
    public bool HasSubmitted { get; set; }

    public QuestionnaireModel(IMongoDatabase database)
    {
        _questionnaires = database.GetCollection<Questionnaire>("Questionnaires");
        _responses = database.GetCollection<QuestionnaireResponse>("QuestionnaireResponses");
    }

    public async Task OnGetAsync()
    {
        // Get the active questionnaire
        ActiveQuestionnaire = await _questionnaires
            .Find(q => q.IsActive)
            .FirstOrDefaultAsync();

        // Check if user has already submitted
        if (User.Identity?.IsAuthenticated == true)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            HasSubmitted = await _responses
                .Find(r => r.QuestionnaireId == ActiveQuestionnaire.Id && r.UserId == userId)
                .AnyAsync();
        }
    }

    public async Task<IActionResult> OnPostSubmitAsync(QuestionnaireResponse response)
    {
        if (!ModelState.IsValid)
        {
            ActiveQuestionnaire = await _questionnaires
                .Find(q => q.IsActive)
                .FirstOrDefaultAsync();
            return Page();
        }

        try
        {
            // Set additional response data
            response.SubmittedAt = DateTime.UtcNow;
            response.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            await _responses.InsertOneAsync(response);

            TempData["SuccessMessage"] = "Thank you for completing the questionnaire!";
            return RedirectToPage();
        }
        catch (Exception)
        {
            ModelState.AddModelError("", "Error submitting your responses");
            // Log the error
            return Page();
        }
    }
}