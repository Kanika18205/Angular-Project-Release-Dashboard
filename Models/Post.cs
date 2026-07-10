using System.ComponentModel.DataAnnotations.Schema;

namespace ReleaseDashboard.Models
{
    public class Post
    {
        public int Id { get; set; }
        public string Version { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? ChangeLog {  get; set; } 
        //one post can be createdBy one user only - One to One mapping
        //one user can have many posts - one to Many Mapping
        public int CreatedBy { get; set; }

        [ForeignKey(nameof(CreatedBy))]
        public User? CreatedByUser { get; set; }

        //Many to Many mapping for Assigning posts to users. 
        public ICollection<ReleaseAssignment> Assignments { get; set; } = new List<ReleaseAssignment>();

        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<PostAttachment> Attachments { get; set; } = new List<PostAttachment>();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
