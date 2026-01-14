using BadmintonShop.Core.Entities;
using BadmintonShop.Core.Interfaces.Services;
using BadmintonShop.Data.DbContext;
using Microsoft.EntityFrameworkCore;

namespace BadmintonShop.Data.Seed
{
    public static class DatabaseSeeder
    {
        public static async Task SeedAdminAsync(
            ApplicationDbContext context,
            IAuthService auth)
        {
            await context.Database.MigrateAsync();

            var adminRole = await context.Roles
                .FirstOrDefaultAsync(r => r.Name == "Admin");

            if (adminRole == null)
            {
                adminRole = new Role { Name = "Admin" };
                context.Roles.Add(adminRole);
                await context.SaveChangesAsync();
            }

            var admin = await context.Users
                .FirstOrDefaultAsync(u => u.Username == "admin");

            if (admin == null)
            {
                admin = new User
                {
                    Username = "admin",
                    FullName = "System Admin",
                    Email = "admin@shop.com",
                    PasswordHash = auth.HashPassword("Admin@123"),
                    RoleId = adminRole.Id,
                    IsActive = true
                };

                context.Users.Add(admin);
                await context.SaveChangesAsync();
            }
        }
    }
}
