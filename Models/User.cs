namespace ReleaseDashboard.Models
{
    public class User
    {
        public int Id { get; set; } //declaring properties for the user data 
        public string Username { get; set; } = "";
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
        public string Role { get; set; } = "User";
        public ICollection<Post> Posts { get; set; } = new List<Post>(); // one to many mapping since one user can create many posts.
        public ICollection<ReleaseAssignment> Assignments { get; set; } = new List<ReleaseAssignment>();
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }
}

