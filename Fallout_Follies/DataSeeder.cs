using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

public class DataSeeder
{
    public static async Task SeedData(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext context)
    {
        // Seed Roles
        await SeedRoles(roleManager);

        // Seed Users
        await SeedUsers(userManager);

        // Seed Products
        await SeedProducts(context);
    }

    private static async Task SeedRoles(RoleManager<IdentityRole> roleManager)
    {
        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            await roleManager.CreateAsync(new IdentityRole("Admin"));
        }
        if (!await roleManager.RoleExistsAsync("User"))
        {
            await roleManager.CreateAsync(new IdentityRole("User"));
        }
    }

    private static async Task SeedUsers(UserManager<ApplicationUser> userManager)
    {
        if (!userManager.Users.Any())
        {
            var adminUser = new ApplicationUser
            {
                UserName = "admin@gmail.com",
                Email = "admin@gmail.com",
                FirstName = "Admin",
                LastName = "Admin"
            };

            await userManager.CreateAsync(adminUser, "AdminPassword123!");
            await userManager.AddToRoleAsync(adminUser, "Admin");

            var testUser1 = new ApplicationUser
            {
                UserName = "testuser1@gmail.com",
                Email = "testuser1@gmail.com",
                FirstName = "Test1",
                LastName = "User"
            };

            await userManager.CreateAsync(testUser1, "TestUserPassword123!");
            await userManager.AddToRoleAsync(testUser1, "User");

            var testUser2 = new ApplicationUser
            {
                UserName = "testuser2@gmail.com",
                Email = "testuser2@gmail.com",
                FirstName = "Test2",
                LastName = "User"
            };

            await userManager.CreateAsync(testUser2, "TestUserPassword123!");
            await userManager.AddToRoleAsync(testUser2, "User");
        }
    }

    private static async Task SeedProducts(ApplicationDbContext context)
    {
        if (!context.Products.Any())
        {
            context.Products.AddRange(
                new Product
                {
                    Name = "Atomic Annihilator",
                    Description = "For those who want to really make an impact...",
                    Price = 10000.00M,
                    ImageUrl = "/assets/images/atomic_annihilator.png",
                    Yield = 50000,
                    Specs = "Proprietary fusion ignition system..."
                },
                // Add more products based on the given data
            );

            await context.SaveChangesAsync();
        }
    }
}
