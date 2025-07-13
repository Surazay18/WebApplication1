using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Driver;

public class AboutModel : PageModel
{
    private readonly IMongoCollection<Questionnaire> _questionnaires;

    public string? ActiveQuestionnaireId { get; set; }

    public AboutModel(IMongoDatabase database)
    {
        _questionnaires = database.GetCollection<Questionnaire>("Questionnaires");
    }

    public async Task OnGetAsync()
    {
        // Get the first active questionnaire
        var activeQuestionnaire = await _questionnaires
            .Find(q => q.IsActive)
            .FirstOrDefaultAsync();

        ActiveQuestionnaireId = activeQuestionnaire?.Id; // Will be null if none exist
    }
}