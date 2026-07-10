using ReleaseDashboard.DTOs.Posts;

namespace ReleaseDashboard.DTOs.Posts
{
    public class PostResponseDto
    {
        public int Id { get; set; }

        public string Version { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string? ChangeLog { get; set; } 

        public string Author { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        // Files attached to this release
        public List<PostAttachmentDto> Attachments { get; set; } = new();

    }
}