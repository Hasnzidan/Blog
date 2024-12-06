using Blog.Data;
using Blog.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Blog.Utilites
{
    public class DbInitializer : IDbInitializer
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public DbInitializer(ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
        }

        public async Task Initialize()
        {
            try
            {
                if (_context.Database.GetPendingMigrations().Any())
                {
                    await _context.Database.MigrateAsync();
                }

                // Add default roles if they don't exist
                var defaultRoles = new[] { "Admin", "User" };
                foreach (var roleName in defaultRoles)
                {
                    if (!string.IsNullOrEmpty(roleName) && !await _roleManager.RoleExistsAsync(roleName))
                    {
                        await _roleManager.CreateAsync(new IdentityRole(roleName));
                    }
                }

                // Add default admin user if no users exist
                if (!_userManager.Users.Any())
                {
                    var admin = new ApplicationUser
                    {
                        UserName = "admin@gmail.com",
                        Email = "admin@gmail.com",
                        FirstName = "Admin",
                        LastName = "User",
                        EmailConfirmed = true
                    };

                    var result = await _userManager.CreateAsync(admin, "Admin123*");
                    if (result.Succeeded)
                    {
                        if (await _roleManager.RoleExistsAsync("Admin"))
                        {
                            await _userManager.AddToRoleAsync(admin, "Admin");
                        }
                    }
                    else
                    {
                        var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                        throw new InvalidOperationException($"Failed to create admin user: {errors}");
                    }
                }

                if (!_context.Pages!.Any())
                {
                    var aboutPage = new Page
                    {
                        Title = "About Us",
                        Slug = "about-us",
                        Content = "Welcome to our blog! This is the about page.",
                        CreatedDate = DateTime.Now,
                        Published = true
                    };

                    await _context.Pages!.AddAsync(aboutPage);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to initialize database", ex);
            }
        }
    }
}
