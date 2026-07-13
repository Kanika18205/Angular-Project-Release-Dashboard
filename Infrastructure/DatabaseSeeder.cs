using Microsoft.EntityFrameworkCore;
using ReleaseDashboard.Data;
using Microsoft.AspNetCore.Identity;
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
            Role = "Admin"
        };

        var hasher = new PasswordHasher<User>();
        
        admin.Password = hasher.HashPassword(admin, "Admin@123");
        
        context.Users.Add(admin);
        
        await context.SaveChangesAsync();
    }
}
