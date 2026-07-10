using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ReleaseDashboard.Data;
using ReleaseDashboard.Models;

namespace ReleaseDashboard.Controllers
{
    [ApiController]
    [Route("api/auth")]

    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;


        public AuthController(AppDbContext context , IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        private string GenerateJwtToken(User user)
        {
            var key = _configuration["Jwt:Key"];

            if (string.IsNullOrWhiteSpace(key))
                throw new InvalidOperationException("JWT Key is Missing");

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));

            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
   {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Name, user.Username),
        new Claim(ClaimTypes.Email, user.Email),
        new Claim(ClaimTypes.Role, user.Role),
    };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        [HttpPost("register")]
        public IActionResult Register (User user)
        {
            var exists = _context.Users.FirstOrDefault(x => x.Email == user.Email);

            if (exists != null)
                return BadRequest("User Already Exists");

            var hasher = new PasswordHasher<User>();
            user.Password = hasher.HashPassword(user, user.Password);

            _context.Users.Add(user);
            _context.SaveChanges();

            return Ok(new { 
                success = true,
                message = "User Registered Successfully"
            });
        }

        [HttpPost("login")]
        public IActionResult Login (User request)
        {
        try{
            var user = _context.Users.FirstOrDefault(x =>
                x.Email == request.Email);

            if(user == null)
            {
                return Unauthorized(new
                {
                    message = "Invalid Credentials"
                });
            }

            var hasher = new PasswordHasher<User>();
            var res = hasher.VerifyHashedPassword(user, user.Password, request.Password);

            if(res == PasswordVerificationResult.Failed)
            {
                return Unauthorized(new
                {
                    message = "Enter Correct Passwords"
                });
            }
            else
            {

                var token = GenerateJwtToken(user);

                return Ok(new
                {
                    token = token,
                    id = user.Id,
                    username = user.Username,
                    email = user.Email,
                    role = user.Role
                });
            }
} catch (Exception ex)
    {
        Console.WriteLine(ex);   // This will appear in Render logs

        return StatusCode(500, ex.ToString());
    }
            

        }
    }
}
