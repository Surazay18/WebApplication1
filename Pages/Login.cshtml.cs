using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using WebApplication1.Models;
using MongoDB.Driver;
using WebApplication1.Services;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Pages
{
    public class LoginModel : PageModel
    {
        private readonly IMongoCollection<User> _usersCollection;
        private readonly PasswordHasher _passwordHasher;
        private readonly ILogger<LoginModel> _logger;

        public LoginModel(
            IMongoDatabase database,
            PasswordHasher passwordHasher,
            ILogger<LoginModel> logger)
        {
            _usersCollection = database.GetCollection<User>("Users");
            _passwordHasher = passwordHasher;
            _logger = logger;
        }

        [BindProperty]
        [Required(ErrorMessage = "Username is required")]
        public string? Username { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        [TempData]
        public string? ErrorMessage { get; set; }

        public IActionResult OnGet()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToUserDashboard();
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                var user = await _usersCollection
                    .Find(u => u.Username == Username)
                    .FirstOrDefaultAsync();

                if (user == null || !_passwordHasher.VerifyHash(Password!, user.PasswordHash, user.PasswordSalt))
                {
                    _logger.LogWarning("Failed login attempt for username: {Username}", Username);
                    ErrorMessage = "Invalid username or password";
                    return Page();
                }

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id ?? string.Empty),
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Role, user.Role),
                    new Claim("LastLogin", DateTime.UtcNow.ToString())
                };

                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(2), // 2 hour session
                    RedirectUri = Url.Content("~/") // Fallback redirect
                };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    principal,
                    authProperties);

                _logger.LogInformation("User {Username} logged in successfully", user.Username);

                return RedirectToUserDashboard(user.Role);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for username: {Username}", Username);
                ErrorMessage = "An error occurred during login. Please try again.";
                return Page();
            }
        }

        private IActionResult RedirectToUserDashboard(string? role = null)
        {
            role ??= User.FindFirst(ClaimTypes.Role)?.Value;

            return role switch
            {
                "Admin" => RedirectToPage("/Admin"),
                _ => RedirectToPage("/SecurePage") // Default for Users
            };
        }
    }
}