namespace ReleaseDashboard.Models
{
    public class ReleaseAssignment
    {
        public int Id { get; set; }
        public int PostId { get; set; }
        public Post? Post { get; set; }
        public int UserId { get; set; }
        public User? User { get; set; }
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    }
}
