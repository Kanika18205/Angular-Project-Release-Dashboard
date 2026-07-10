using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using ReleaseDashboard.Data;
using ReleaseDashboard.DTOs.Notifications;
using ReleaseDashboard.Hubs;
using ReleaseDashboard.Models;

namespace ReleaseDashboard.Services
{
    public class NotificationService : INotificationService
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<NotificationHub> _hub;

        public NotificationService(
            AppDbContext context,
            IHubContext<NotificationHub> hub)
        {
            _context = context;
            _hub = hub;
        }

        public async Task NotifyReleaseCreatedAsync(Post post, int triggeredById)
        {
            var users = await _context.Users
                .Where(u => u.Role == "User")
                .ToListAsync();

            var notifications = users.Select(user => new Notification
            {
                UserId = user.Id,
                TriggeredById = triggeredById,
                PostId = post.Id,
                Type = NotificationType.ReleaseCreated,
                Title = "New Release",
                Message = $"Version {post.Version} has been published. ",
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            }).ToList();
            _context.Notifications.AddRange(notifications);
            await _context.SaveChangesAsync();

            foreach (var notification in notifications)
            {
                await _hub.Clients
                    .User(notification.UserId.ToString())
                    .SendAsync(
                        "ReceiveNotification",

                        new NotificationDto
                        {
                            Id = notification.Id,
                            PostId = notification.PostId,
                            Title = notification.Title,
                            Message = notification.Message,
                            Type = (int)notification.Type,
                            IsRead = notification.IsRead,
                            CreatedAt = notification.CreatedAt
                        });
            }
        }

        public async Task NotifyReleaseUpdatedAsync(Post post, int triggeredById)
        {
            var assignedUsers = await _context.ReleaseAssignments
                   .Where(a => a.PostId == post.Id)
                   .Select(a => a.UserId)
                   .ToListAsync();

            var notifications = assignedUsers.Select(userId => new Notification
            {
                UserId = userId,
                TriggeredById = triggeredById,
                PostId = post.Id,
                Type = NotificationType.ReleaseUpdated,
                Title = "Release Updated",
                Message = $"Release {post.Version} has been updated.",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            });

            _context.Notifications.AddRange(notifications);

            await _context.SaveChangesAsync();

            foreach (var notification in notifications)
            {
                await _hub.Clients
                    .User(notification.UserId.ToString())
                    .SendAsync(
                        "ReceiveNotification",

                        new NotificationDto
                        {
                            Id = notification.Id,
                            PostId = notification.PostId,
                            Title = notification.Title,
                            Message = notification.Message,
                            Type = (int)notification.Type,
                            IsRead = notification.IsRead,
                            CreatedAt = notification.CreatedAt
                        });
            }
        }

        public async Task NotifyUsersAssignedAsync(int postId, List<int> assignedUserIds, int triggeredById)
        {
            var post = await _context.Posts.FindAsync(postId);
            if (post == null)
                return;
            var notifications = assignedUserIds.Select(userId => new Notification
            {
                UserId = userId,
                TriggeredById = triggeredById,
                PostId = postId,
                Type = NotificationType.Assignment,
                Title = "Release Assigned",
                Message = $"You have been assigned Release {post.Version}.",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            });

            _context.Notifications.AddRange(notifications);
            await _context.SaveChangesAsync();

            foreach (var notification in notifications)
            {
                await _hub.Clients
                    .User(notification.UserId.ToString())
                    .SendAsync(
                        "ReceiveNotification",

                        new NotificationDto
                        {
                            Id = notification.Id,
                            PostId = notification.PostId,
                            Title = notification.Title,
                            Message = notification.Message,
                            Type = (int)notification.Type,
                            IsRead = notification.IsRead,
                            CreatedAt = notification.CreatedAt
                        });
            }
        }

        public async Task NotifyCommentAddedAsync(Comment comment, int triggeredById)
        {
            var admin = await _context.Users
                    .FirstOrDefaultAsync(u => u.Role == "Admin");

            if (admin == null)
                return;

            var triggeredBy = await _context.Users
                .FirstAsync(u => u.Id == triggeredById);

            var notification = new Notification
            {
                UserId = admin.Id,
                TriggeredById = triggeredById,
                PostId = comment.PostId,
                Type = NotificationType.CommentAdded,
                Title = "New Comment",
                Message = $"{triggeredBy.Username} added a comment.",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.Notifications.Add(notification);

            await _context.SaveChangesAsync();

           
            await _hub.Clients
                .User(notification.UserId.ToString())
                .SendAsync(
                    "ReceiveNotification",

                    new NotificationDto
                    {
                        Id = notification.Id,
                        PostId = notification.PostId,
                        Title = notification.Title,
                        Message = notification.Message,
                        Type = (int)notification.Type,
                        IsRead = notification.IsRead,
                        CreatedAt = notification.CreatedAt
                    });
            
        }

        public async Task NotifyCommentDeletedAsync(Comment comment, int triggeredById)
        {
            var admin = await _context.Users.FirstOrDefaultAsync(u => u.Role == "Admin");
            if(admin == null) return;

            var triggeredBy = await _context.Users.FirstAsync(u => u.Id == triggeredById);
            var notification = new Notification
            {
                UserId = admin.Id,
                TriggeredById = triggeredById,
                PostId = comment.PostId,
                Type = NotificationType.CommentDeleted,
                Title = "Comment Deleted",
                Message = $"{triggeredBy.Username} deleted a comment.",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            await _hub.Clients
                .User(notification.UserId.ToString())
                .SendAsync(
                    "ReceiveNotification",

                    new NotificationDto
                    {
                        Id = notification.Id,
                        PostId = notification.PostId,
                        Title = notification.Title,
                        Message = notification.Message,
                        Type = (int)notification.Type,
                        IsRead = notification.IsRead,
                        CreatedAt = notification.CreatedAt
                    });
            

        }


        public async Task<List<NotificationResponseDto>> GetNotificationsAsync(int postId, int userId)
        {
            return await _context.Notifications
                .Where(n => n.PostId == postId && n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Select(n => new NotificationResponseDto
                {
                    Id = n.Id,
                    PostId = n.PostId,
                    Title = n.Title,
                    Message = n.Message,
                    Type = (int)n.Type,
                    IsRead = n.IsRead,
                    CreatedAt = n.CreatedAt
                }).ToListAsync();
        }

        public async Task MarkAsReadAsync( int postId,int userId)
        {
            var notifications = await _context.Notifications
                .Where(n => n.PostId == postId && n.UserId == userId && !n.IsRead)
                .ToListAsync();

            foreach (var notification in notifications)
            {
                notification.IsRead = true;
            }

            await _context.SaveChangesAsync();
        }
    }
}
