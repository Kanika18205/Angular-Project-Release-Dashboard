namespace ReleaseDashboard.DTOs.Comments
{
    public class CommentResponseDto
    {
        public int Id { get; set; }
        public string? Text { get; set; }
        public string Author { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public List<CommentAttachmentDto> Attachments { get; set; } = new();
    }
}
