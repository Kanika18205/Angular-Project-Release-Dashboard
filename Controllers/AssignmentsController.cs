using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using ReleaseDashboard.Data;
using ReleaseDashboard.DTOs.Assignments;
using ReleaseDashboard.Models;
using ReleaseDashboard.Services;

namespace ReleaseDashboard.Controllers
{
    [ApiController]
    [Route("api/assignments")]
    [Authorize]
    public class AssignmentsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly INotificationService _notificationService;

        public AssignmentsController(AppDbContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        // Get Assigned Users
        [HttpGet("post/{postId}")]
        public async Task<IActionResult> GetAssignments(int postId)
        {
            var assignments = await _context.ReleaseAssignments
                .Include(a => a.User)
                .Where(a => a.PostId == postId)
                .Select(a => new AssignmentResponseDto
                {
                    Id = a.Id,
                    UserId = a.UserId,
                    Username = a.User!.Username,
                    Email = a.User.Email,
                    AssignedAt = a.AssignedAt
                }).ToListAsync();

            return Ok(assignments);
        }

        // Get All Users
        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _context.Users
                .Where(u => u.Role != "Admin")
                .Select(u => new
                {
                    id = u.Id,
                    username = u.Username,
                    email = u.Email,
                    role = u.Role
                })
                .ToListAsync();

            return Ok(users);
        }

        // Assign User
        [HttpPost]
        public async Task<IActionResult> AssignUser(AssignUserDto dto)
        {
            foreach (var userId in dto.UserIds)
            {
                bool alreadyAssigned = await _context.ReleaseAssignments
                    .AnyAsync(a => a.PostId == dto.PostId &&
                                   a.UserId == userId);

                if (alreadyAssigned)
                    continue;

                var assignment = new ReleaseAssignment
                {
                    PostId = dto.PostId,
                    UserId = userId,
                    AssignedAt = DateTime.UtcNow
                };

                _context.ReleaseAssignments.Add(assignment);
            }

            await _context.SaveChangesAsync();
            await _notificationService.NotifyUsersAssignedAsync(dto.PostId, dto.UserIds, int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value));

            return Ok(new
            {
                message = "Users assigned successfully."
            });
        }

        // Remove Assignment
        [HttpDelete("{id}")]
        public async Task<IActionResult> RemoveAssignment(int id)
        {
            var assignment =
                await _context.ReleaseAssignments.FindAsync(id);

            if (assignment == null)
                return NotFound();

            _context.ReleaseAssignments.Remove(assignment);

            await _context.SaveChangesAsync();
            return Ok(new
            {
                message =
                    "Assignment removed successfully."
            });
        }
    }
}