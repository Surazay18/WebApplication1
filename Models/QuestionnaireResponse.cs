// Models/QuestionnaireResponse.cs
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class QuestionnaireResponse
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    public string QuestionnaireId { get; set; } = null!;

    public string UserId { get; set; } = null!;
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    public List<QuestionAnswer> Answers { get; set; } = new();
}

public class QuestionAnswer
{
    public int QuestionIndex { get; set; }
    public string? Answer { get; set; }
    public List<string>? Answers { get; set; }
}