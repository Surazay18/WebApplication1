using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Bson;
using MongoDB.Driver;
using WebApplication1.Models;

namespace WebApplication1.Pages
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly IMongoCollection<Member> _membersCollection;

        public long TotalMembersCount { get; set; }
        public long ActiveMembersCount { get; set; }
        public long NewMembersThisMonth { get; set; }

        public IndexModel(IMongoCollection<Member> membersCollection)
        {
            _membersCollection = membersCollection;
        }
        // Add property for membership type data
        public Dictionary<string, int> MembershipTypeCounts { get; set; } = new();

        public async Task OnGetAsync()
        {
            // Get membership type distribution
            var pipeline = new BsonDocument[]
            {
                new BsonDocument("$group",
                    new BsonDocument
                    {
                        { "_id", "$MembershipType" },
                        { "count", new BsonDocument("$sum", 1) }
                    }),
                new BsonDocument("$project",
                    new BsonDocument
                    {
                        { "Type", "$_id" },
                        { "Count", "$count" },
                        { "_id", 0 }
                    })
            };

            var results = await _membersCollection.Aggregate<BsonDocument>(pipeline).ToListAsync();

            MembershipTypeCounts = results.ToDictionary(
                x => x["Type"].AsString,
                x => x["Count"].AsInt32
            );

            var countsTask = Task.WhenAll(
               _membersCollection.CountDocumentsAsync(FilterDefinition<Member>.Empty),
               _membersCollection.CountDocumentsAsync(Builders<Member>.Filter.Eq(m => m.IsActive, true)),
               _membersCollection.CountDocumentsAsync(Builders<Member>.Filter.Gte(m => m.JoinDate, DateTime.UtcNow.AddMonths(-1)))
           );

            var counts = await countsTask;
            TotalMembersCount = counts[0];
            ActiveMembersCount = counts[1];
            NewMembersThisMonth = counts[2];
        }
       
    }
}