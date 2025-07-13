using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

public class DisplayQuestionnaireModel : PageModel
{
    [BindProperty]
    public Questionnaire Questionnaire { get; set; } = null!;

    [BindProperty]
    public string QuestionnaireId { get; set; } = null!;

    [BindProperty]
    public List<AnswerInput> Answers { get; set; } = new();

    public void OnGet(string id)
    {
        // Load questionnaire from database
        QuestionnaireId = id;
        // This is just sample data - replace with actual DB call
        Questionnaire = new Questionnaire
        {
            Id = id,
            Title = "Sample Questionnaire",
            Description = "This is a sample questionnaire",
            Questions = new List<Question>
            {
                new Question
                {
                    Text = "What is your name?",
                    Type = QuestionType.Text,
                    IsRequired = true
                },
                new Question
                {
                    Text = "Choose your favorite color",
                    Type = QuestionType.Radio,
                    IsRequired = true,
                    Options = new List<string> { "Red", "Blue", "Green" }
                },
                new Question
                {
                    Text = "Select your hobbies",
                    Type = QuestionType.Checkbox,
                    Options = new List<string> { "Reading", "Sports", "Music" }
                },
                new Question
                {
                    Text = "How would you rate our service?",
                    Type = QuestionType.Rating,
                    IsRequired = true
                }
            }
        };

        // Initialize answers list
        Answers = Questionnaire.Questions.Select(q => new AnswerInput()).ToList();
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var userAnswer = new UserAnswer
        {
            QuestionnaireId = QuestionnaireId,
            UserId = User.Identity?.Name ?? "anonymous",
            SubmittedAt = DateTime.UtcNow,
            Answers = new List<Answer>()
        };

        for (int i = 0; i < Answers.Count; i++)
        {
            var answerInput = Answers[i];
            var question = Questionnaire.Questions[i];

            var answer = new Answer
            {
                QuestionIndex = i
            };

            switch (question.Type)
            {
                case QuestionType.Text:
                    answer.TextResponse = answerInput.TextResponse;
                    break;
                case QuestionType.Radio:
                case QuestionType.Dropdown:
                    answer.SingleOptionResponse = answerInput.SingleOptionResponse;
                    break;
                case QuestionType.Checkbox:
                    answer.MultipleOptionsResponse = answerInput.MultipleOptionsResponse;
                    break;
                case QuestionType.Rating:
                    answer.RatingResponse = answerInput.RatingResponse;
                    break;
            }

            userAnswer.Answers.Add(answer);
        }

        // Save to database
        // await _userAnswerRepository.CreateAsync(userAnswer);

        return RedirectToPage("/ThankYou");
    }
}

public class AnswerInput
{
    public string? TextResponse { get; set; }
    public string? SingleOptionResponse { get; set; }
    public List<string>? MultipleOptionsResponse { get; set; }
    public int? RatingResponse { get; set; }
}