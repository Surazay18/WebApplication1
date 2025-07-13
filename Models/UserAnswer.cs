using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class UserAnswer
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    public string QuestionnaireId { get; set; } = null!;

    public string UserId { get; set; } = null!;
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    public List<Answer> Answers { get; set; } = new();
}

public class Answer
{
    public int QuestionIndex { get; set; }
    public string? TextResponse { get; set; }
    public string? SingleOptionResponse { get; set; }
    public List<string>? MultipleOptionsResponse { get; set; }
    public int? RatingResponse { get; set; }
}