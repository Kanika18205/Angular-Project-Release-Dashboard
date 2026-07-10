using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using ReleaseDashboard.Data;
using ReleaseDashboard.Models;

namespace ReleaseDashboard.Infrastructure;

public static class DatabaseSeeder
{
    public static async Task SeedAdminAsync(AppDbContext context)
    {
        if (await context.Users.AnyAsync(u => u.Email == "admin@example.com"))
            return;

        var admin = new User
        {
            Username = "admin",
            Email = "admin@example.com",
            Password = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
            Role = "Admin"
        };

        context.Users.Add(admin);
        await context.SaveChangesAsync();
    }
}
