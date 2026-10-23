using EmployeeManagementAPI.DTOs;
using EmployeeManagementAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace EmployeeManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly EmployeeDbContext _context;
        private readonly PasswordHasher<ApplicationUser> _hasher;
        private readonly IConfiguration _configuration;
       public AuthController(EmployeeDbContext context,IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            _hasher = new PasswordHasher<ApplicationUser>();
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            bool exists = await _context.Users
                .AnyAsync(u => u.Username == request.Username);

            if (exists)
                return Conflict(new { message = "Username already exists." });

            var user = new ApplicationUser
            {
                Username = request.Username,
                Role = "Employee"
            };

            user.PasswordHash = _hasher.HashPassword(
                user, request.Password);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "User registered successfully." });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == request.Username);

            if (user == null)
                return NotFound(new { message = "User not found." });

            var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);

            if (result == PasswordVerificationResult.Failed)
                return Unauthorized(new { message = "Invalid password." });

            var token = GenerateJwtToken(user);

            return Ok(new
            {
                message = "Login successful.",
                username = user.Username,
                token = token
            });
        }
        private string GenerateJwtToken(ApplicationUser user)
{
    var key = _configuration["Jwt:Key"]
        ?? throw new InvalidOperationException("JWT key is missing.");

    var issuer = _configuration["Jwt:Issuer"]
        ?? throw new InvalidOperationException("JWT issuer is missing.");

    var audience = _configuration["Jwt:Audience"]
        ?? throw new InvalidOperationException("JWT audience is missing.");

    var expiryMinutes = _configuration.GetValue<int>("Jwt:ExpiryMinutes");

    var claims = new[]
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Name, user.Username),
        new Claim(ClaimTypes.Role, user.Role)
    };

    var securityKey = new SymmetricSecurityKey(
        Encoding.UTF8.GetBytes(key));

    var credentials = new SigningCredentials(
        securityKey,
        SecurityAlgorithms.HmacSha256);

    var token = new JwtSecurityToken(
        issuer: issuer,
        audience: audience,
        claims: claims,
        expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
        signingCredentials: credentials);

    return new JwtSecurityTokenHandler().WriteToken(token);
}
    }
}