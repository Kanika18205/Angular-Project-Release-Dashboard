using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using ReleaseDashboard.Data;
using ReleaseDashboard.DTOs.Users;

namespace ReleaseDashboard.Controllers
{
    [ApiController]
    [Route("api/users")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsersController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _context.Users
                .Where(u=> u.Role == "User")
                .Select(u => new
                {
                    u.Id,
                    u.Username,
                    u.Email,
                    AssignedCount = _context.ReleaseAssignments.Count(a=> a.UserId == u.Id)
                }).OrderBy(u => u.Username)
                .ToListAsync();

            return Ok(users);
        }

        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile(UpdateProfileDto dto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            user.Username = dto.Username;
            user.Email = dto.Email;

            await _context.SaveChangesAsync();

            return Ok(new { 
                user.Username,
                user.Email,
                user.Role
            });
        }
    }
}