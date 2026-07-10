namespace ReleaseDashboard.DTOs.Assignments
{
    public class AssignmentResponseDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime AssignedAt { get; set; }
    }
}