using System.ComponentModel.DataAnnotations;

namespace ReleaseDashboard.DTOs.Comments
{
    public class CreateCommentDto
    {
        [Required]
        public int PostId { get; set; }
        public string? Text { get; set; }
        public List<IFormFile> Files { get; set; } = new();
    }
}
