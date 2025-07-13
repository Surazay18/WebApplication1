using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace WebApplication1.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class QuestionnaireModel : PageModel
    {
        private readonly IMongoCollection<Questionnaire> _questionnaires;
        private readonly ILogger<QuestionnaireModel> _logger;

        [BindProperty]
        public Questionnaire CurrentQuestionnaire { get; set; } = new();

        [BindProperty]
        public Question NewQuestion { get; set; } = new();

        [BindProperty]
        public List<Questionnaire> ExistingQuestionnaires { get; set; } = new();

        public QuestionnaireModel(IMongoDatabase database, ILogger<QuestionnaireModel> logger)
        {
            _questionnaires = database.GetCollection<Questionnaire>("Questionnaires");
            _logger = logger;

            // Ensure indexes
            CreateIndexes();
        }

        private void CreateIndexes()
        {
            try
            {
                var indexKeys = Builders<Questionnaire>.IndexKeys.Ascending(q => q.Title);
                var indexOptions = new CreateIndexOptions { Unique = false };
                _questionnaires.Indexes.CreateOne(new CreateIndexModel<Questionnaire>(indexKeys, indexOptions));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating indexes");
            }
        }

        public async Task OnGetAsync(string? id)
        {
            try
            {
                ExistingQuestionnaires = await _questionnaires.Find(_ => true).ToListAsync();

                if (!string.IsNullOrEmpty(id))
                {
                    CurrentQuestionnaire = await _questionnaires.Find(q => q.Id == id).FirstOrDefaultAsync()
                                        ?? new Questionnaire();

                    // Ensure Questions list is never null
                    CurrentQuestionnaire.Questions ??= new List<Question>();

                    _logger.LogInformation($"Loaded questionnaire with {CurrentQuestionnaire.Questions.Count} questions");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading questionnaires");
                ModelState.AddModelError("", "Error loading data");
            }
        }

        public async Task<IActionResult> OnPostAddQuestionAsync()
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    await LoadExistingQuestionnaires();
                    return Page();
                }

                // Initialize Questions list if null
                CurrentQuestionnaire.Questions ??= new List<Question>();

                // Set default order if not specified
                if (NewQuestion.Order == 0)
                {
                    NewQuestion.Order = CurrentQuestionnaire.Questions.Count + 1;
                }

                CurrentQuestionnaire.Questions.Add(NewQuestion);
                _logger.LogInformation($"Added question. Total questions now: {CurrentQuestionnaire.Questions.Count}");

                // Reset for next question
                NewQuestion = new Question();

                await LoadExistingQuestionnaires();
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding question");
                ModelState.AddModelError("", "Error adding question");
                await LoadExistingQuestionnaires();
                return Page();
            }
        }

        public async Task<IActionResult> OnPostRemoveQuestionAsync(int index)
        {
            try
            {
                if (CurrentQuestionnaire.Questions != null &&
                    index >= 0 &&
                    index < CurrentQuestionnaire.Questions.Count)
                {
                    CurrentQuestionnaire.Questions.RemoveAt(index);
                    _logger.LogInformation($"Removed question at index {index}");
                }

                await LoadExistingQuestionnaires();
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing question");
                ModelState.AddModelError("", "Error removing question");
                await LoadExistingQuestionnaires();
                return Page();
            }
        }

        public async Task<IActionResult> OnPostSaveAsync()
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    await LoadExistingQuestionnaires();
                    return Page();
                }

                // Ensure Questions list is initialized
                CurrentQuestionnaire.Questions ??= new List<Question>();

                // Set timestamps
                CurrentQuestionnaire.UpdatedAt = DateTime.UtcNow;
                if (string.IsNullOrEmpty(CurrentQuestionnaire.Id))
                {
                    CurrentQuestionnaire.CreatedAt = DateTime.UtcNow;
                }

                // If activating, deactivate all others first
                if (CurrentQuestionnaire.IsActive)
                {
                    await _questionnaires.UpdateManyAsync(
                        Builders<Questionnaire>.Filter.Ne(q => q.Id, CurrentQuestionnaire.Id),
                        Builders<Questionnaire>.Update.Set(q => q.IsActive, false));
                }

                // Save or update
                if (string.IsNullOrEmpty(CurrentQuestionnaire.Id))
                {
                    await _questionnaires.InsertOneAsync(CurrentQuestionnaire);
                    TempData["SuccessMessage"] = "Questionnaire created successfully!";
                    _logger.LogInformation($"Created new questionnaire with ID: {CurrentQuestionnaire.Id}");
                }
                else
                {
                    await _questionnaires.ReplaceOneAsync(
                        q => q.Id == CurrentQuestionnaire.Id,
                        CurrentQuestionnaire,
                        new ReplaceOptions { IsUpsert = true });

                    TempData["SuccessMessage"] = "Questionnaire updated successfully!";
                    _logger.LogInformation($"Updated questionnaire with ID: {CurrentQuestionnaire.Id}");
                }

                _logger.LogInformation($"Saved questionnaire with {CurrentQuestionnaire.Questions.Count} questions");

                return RedirectToPage(new { id = CurrentQuestionnaire.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving questionnaire");
                ModelState.AddModelError("", "Error saving questionnaire");
                await LoadExistingQuestionnaires();
                return Page();
            }
        }

        public async Task<IActionResult> OnPostDeleteAsync(string id)
        {
            try
            {
                await _questionnaires.DeleteOneAsync(q => q.Id == id);
                TempData["SuccessMessage"] = "Questionnaire deleted successfully!";
                _logger.LogInformation($"Deleted questionnaire with ID: {id}");
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting questionnaire");
                TempData["ErrorMessage"] = "Error deleting questionnaire";
                return RedirectToPage();
            }
        }

        public async Task<IActionResult> OnPostToggleActiveAsync(string id)
        {
            try
            {
                var questionnaire = await _questionnaires.Find(q => q.Id == id).FirstOrDefaultAsync();
                if (questionnaire != null)
                {
                    // If activating, deactivate all others first
                    if (!questionnaire.IsActive)
                    {
                        await _questionnaires.UpdateManyAsync(
                            Builders<Questionnaire>.Filter.Ne(q => q.Id, id),
                            Builders<Questionnaire>.Update.Set(q => q.IsActive, false));
                    }

                    var update = Builders<Questionnaire>.Update
                        .Set(q => q.IsActive, !questionnaire.IsActive)
                        .Set(q => q.UpdatedAt, DateTime.UtcNow);

                    await _questionnaires.UpdateOneAsync(q => q.Id == id, update);
                    _logger.LogInformation($"Toggled active status for questionnaire ID: {id}");
                }
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling questionnaire status");
                TempData["ErrorMessage"] = "Error changing questionnaire status";
                return RedirectToPage();
            }
        }

        private async Task LoadExistingQuestionnaires()
        {
            ExistingQuestionnaires = await _questionnaires.Find(_ => true).ToListAsync();
        }
    }

    public class Questionnaire
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("Title")]
        [Required(ErrorMessage = "Title is required")]
        public string Title { get; set; } = string.Empty;

        [BsonElement("Description")]
        public string Description { get; set; } = string.Empty;

        [BsonElement("IsActive")]
        public bool IsActive { get; set; }

        [BsonElement("Questions")]
        public List<Question> Questions { get; set; } = new List<Question>();

        [BsonElement("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("UpdatedAt")]
        public DateTime? UpdatedAt { get; set; }
    }

    public class Question
    {
        [BsonElement("Text")]
        [Required(ErrorMessage = "Question text is required")]
        public string Text { get; set; } = string.Empty;

        [BsonElement("Type")]
        public QuestionType Type { get; set; } = QuestionType.Text;

        [BsonElement("Options")]
        public List<string> Options { get; set; } = new List<string>();

        [BsonElement("IsRequired")]
        public bool IsRequired { get; set; }

        [BsonElement("Order")]
        public int Order { get; set; }
    }

    public enum QuestionType
    {
        Text,
        Radio,
        Checkbox,
        Dropdown,
        Rating
    }
}