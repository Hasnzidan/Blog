using Blog.Models;
using Blog.Utilites;
using Microsoft.AspNetCore.Identity;

namespace Blog.Data
{
    public class DbInitializer : IDbInitializer
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public DbInitializer(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public void Initialize()
        {
            try
            {
                if (_context.Database.CanConnect())
                {
                    if (!_roleManager.RoleExistsAsync(WebsiteRoles.WebsiteAdmin).GetAwaiter().GetResult())
                    {
                        _roleManager.CreateAsync(new IdentityRole(WebsiteRoles.WebsiteAdmin)).GetAwaiter().GetResult();
                        _roleManager.CreateAsync(new IdentityRole(WebsiteRoles.WebsiteUser)).GetAwaiter().GetResult();

                        // Create Admin User
                        var adminUser = new ApplicationUser
                        {
                            UserName = "admin@blog.com",
                            Email = "admin@blog.com",
                            FirstName = "Admin",
                            LastName = "User"
                        };

                        _userManager.CreateAsync(adminUser, "Admin@123").GetAwaiter().GetResult();
                        _userManager.AddToRoleAsync(adminUser, WebsiteRoles.WebsiteAdmin).GetAwaiter().GetResult();
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
