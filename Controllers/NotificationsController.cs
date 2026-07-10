using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReleaseDashboard.Services;
using System.Security.Claims;

namespace ReleaseDashboard.Controllers
{
    [ApiController]
    [Route("api/notifications")]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationsController(
            INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpGet("post/{postId}")]
        public async Task<IActionResult> GetNotifications(
            int postId)
        {
            int userId = int.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var notifications =
                await _notificationService
                    .GetNotificationsAsync(postId, userId);

            return Ok(notifications);
        }

        [HttpPut("post/{postId}/read")]
        public async Task<IActionResult> MarkAsRead(
            int postId)
        {
            int userId = int.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            await _notificationService
                .MarkAsReadAsync(postId, userId);

            return Ok(new
            {
                message = "Notifications marked as read."
            });
        }
    }
}