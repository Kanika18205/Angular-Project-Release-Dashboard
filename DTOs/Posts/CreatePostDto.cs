using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace ReleaseDashboard.DTOs.Posts
{
    public class CreatePostDto
    {
        [Required]
        public string Version { get; set; } = string.Empty;

        [Required]
        public string Title { get; set; } = string.Empty;

        public string? ChangeLog { get; set; }

        public List<IFormFile> Files { get; set; } = new();
    }
}