namespace ReleaseDashboard.Models
{
    public class PostAttachment
    {
        public int Id { get; set; }
        public int PostId { get; set; }
        public Post? Post { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string StoredFileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public bool IsCurrent { get; set; } = true;
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    }
}