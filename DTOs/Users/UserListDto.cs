namespace ReleaseDashboard.DTOs.Users
{
    public class UserListDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int AssignedCount { get; set; }
    }
}
