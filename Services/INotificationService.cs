using ReleaseDashboard.DTOs.Notifications;
using ReleaseDashboard.Models;

namespace ReleaseDashboard.Services
{
    public interface INotificationService
    {
        Task NotifyReleaseCreatedAsync(Post post, int triggeredById);
        Task NotifyReleaseUpdatedAsync(Post post, int triggeredById);
        Task NotifyUsersAssignedAsync(int postId, List<int> assignedUserIds ,int triggeredById);
        Task NotifyCommentAddedAsync(Comment comment, int triggeredById);
        Task NotifyCommentDeletedAsync(Comment comment, int triggeredById);
        Task <List<NotificationResponseDto>> GetNotificationsAsync(int postId, int userId);
        Task MarkAsReadAsync(int postId, int userId);

    }
}
