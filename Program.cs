using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using WebApplication1.Models;
using WebApplication1.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();



// Add Cookie Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login";
        options.LogoutPath = "/Logout";
        options.AccessDeniedPath = "/AccessDenied";
    });

// Add PasswordHasher
builder.Services.AddSingleton<PasswordHasher>();

// MongoDB Configuration
builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    return new MongoClient(config["MongoDBSettings:ConnectionString"]);
});

builder.Services.AddSingleton<IMongoDatabase>(sp =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    var config = sp.GetRequiredService<IConfiguration>();
    return client.GetDatabase(config["MongoDBSettings:DatabaseName"]);
});

// Register collections explicitly
builder.Services.AddSingleton<IMongoCollection<Questionnaire>>(sp =>
{
    var database = sp.GetRequiredService<IMongoDatabase>();
    return database.GetCollection<Questionnaire>("Questionnaires");
});


builder.Services.AddSingleton<IMongoCollection<QuestionnaireResponse>>(sp =>
{
    var database = sp.GetRequiredService<IMongoDatabase>();
    return database.GetCollection<QuestionnaireResponse>("QuestionnaireResponses");
});

// Add this with your other collection registrations
builder.Services.AddSingleton<IMongoCollection<Member>>(sp =>
{
    var database = sp.GetRequiredService<IMongoDatabase>();
    return database.GetCollection<Member>("Members");
});

// Add session support (required for your questionnaire editor)
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});




var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Add session middleware (must come before UseRouting)
app.UseSession();

app.UseRouting();

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// Ensure collections exist on startup
using (var scope = app.Services.CreateScope())
{
    var database = scope.ServiceProvider.GetRequiredService<IMongoDatabase>();

    try
    {
        // This will create the collections if they don't exist
        var questionnaireCollection = database.GetCollection<Questionnaire>("Questionnaires");
        var responseCollection = database.GetCollection<QuestionnaireResponse>("QuestionnaireResponses");

        // Add Members collection
        var membersCollection = database.GetCollection<Member>("Members");

        // Create indexes for Members
        var memberEmailIndexKeys = Builders<Member>.IndexKeys.Ascending(x => x.Email);
        var memberEmailIndexOptions = new CreateIndexOptions { Name = "Email_Index", Unique = true };
        await membersCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<Member>(memberEmailIndexKeys, memberEmailIndexOptions));

        // Optional: Create indexes if needed
        var questionnaireIndexKeys = Builders<Questionnaire>.IndexKeys.Ascending(x => x.IsActive);
        var questionnaireIndexOptions = new CreateIndexOptions { Name = "IsActive_Index" };
        await questionnaireCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<Questionnaire>(questionnaireIndexKeys, questionnaireIndexOptions));
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while setting up MongoDB collections");
    }
}

app.MapRazorPages();

app.Run();