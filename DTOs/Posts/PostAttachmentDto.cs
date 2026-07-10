namespace ReleaseDashboard.DTOs.Posts
{
    public class PostAttachmentDto
    {
        public int Id { get; set; }

        // Original filename shown to the user
        public string FileName { get; set; } = string.Empty;

        // URL used by Angular to download/view the file
        public string FileUrl { get; set; } = string.Empty;

        // MIME type
        public string ContentType { get; set; } = string.Empty;

        // Size in bytes
        public long FileSize { get; set; }
    }
}