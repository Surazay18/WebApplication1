// Models/Questionnaire.cs
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

public class Questionnaire
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("Title")]
    [Required(ErrorMessage = "Title is required")]
    [StringLength(100, ErrorMessage = "Title cannot exceed 100 characters")]
    public string Title { get; set; } = string.Empty;

    [BsonElement("Description")]
    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string Description { get; set; } = string.Empty;

    [BsonElement("IsActive")]
    public bool IsActive { get; set; } = false;

    [BsonElement("CreatedAt")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("UpdatedAt")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime? UpdatedAt { get; set; }

    [BsonElement("Questions")]
    [BsonIgnoreIfNull] // Prevents storing null arrays
    public List<Question> Questions { get; set; } = new List<Question>();
}

// Models/Question.cs
public class Question
{
    [BsonElement("Text")]
    [Required(ErrorMessage = "Question text is required")]
    [StringLength(500, ErrorMessage = "Question text cannot exceed 500 characters")]
    public string Text { get; set; } = string.Empty;

    [BsonElement("Type")]
    [BsonRepresentation(BsonType.String)] // Stores enum as string
    public QuestionType Type { get; set; } = QuestionType.Text;

    [BsonElement("Options")]
    [BsonIgnoreIfNull]
    public List<string> Options { get; set; } = new List<string>();

    [BsonElement("IsRequired")]
    public bool IsRequired { get; set; } = false;

    [BsonElement("Order")]
    [Range(0, 1000, ErrorMessage = "Order must be between 0 and 1000")]
    public int Order { get; set; } = 0;
}

public enum QuestionType
{
    Text,
    Radio,
    Checkbox,
    Dropdown,
    Rating
}