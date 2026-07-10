using System.ComponentModel.DataAnnotations;

namespace ReleaseDashboard.DTOs.Assignments
{
    public class AssignUserDto
    {
        [Required]
        public int PostId { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "Select at least one user.")]
        public List<int> UserIds { get; set; } = new();
    }
}