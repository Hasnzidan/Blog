using Blog.Data;
using Blog.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Blog.Utilities
{
    public class DbSeeder
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DbSeeder> _logger;

        public DbSeeder(ApplicationDbContext context, ILogger<DbSeeder> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task SeedAsync()
        {
            try
            {
                await _context.Database.MigrateAsync();

                if (!_context.Categories.Any())
                {
                    _logger.LogInformation("Seeding categories...");
                    await SeedCategoriesAsync();
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Categories seeded successfully");
                }

                if (!_context.Tags.Any())
                {
                    _logger.LogInformation("Seeding tags...");
                    await SeedTagsAsync();
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Tags seeded successfully");
                }

                if (!_context.Pages!.Any())
                {
                    _logger.LogInformation("Seeding pages...");
                    await SeedPagesAsync();
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Pages seeded successfully");
                }

                _logger.LogInformation("Database seeding completed successfully");
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "An error occurred while saving seeded data to the database");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while seeding the database");
                throw;
            }
        }

        private async Task SeedCategoriesAsync()
        {
            var categories = new List<Category>
            {
                new Category
                {
                    NameAr = "تطوير الويب",
                    NameEn = "Web Development",
                    Slug = "web-development"
                },
                new Category
                {
                    NameAr = "تطوير تطبيقات الموبايل",
                    NameEn = "Mobile Development",
                    Slug = "mobile-development"
                }
            };

            await _context.Categories.AddRangeAsync(categories);
        }

        private async Task SeedTagsAsync()
        {
            var tags = new List<Tag>
            {
                new Tag
                {
                    NameAr = "سي شارب",
                    NameEn = "C#",
                    Slug = "csharp"
                },
                new Tag
                {
                    NameAr = "ايه اس بي نت كور",
                    NameEn = "ASP.NET Core",
                    Slug = "aspnet-core"
                }
            };

            await _context.Tags.AddRangeAsync(tags);
        }

        private async Task SeedPagesAsync()
        {
            var pages = new List<Page>
            {
                new Page
                {
                    Title = "About Us",
                    Slug = "about",
                    ShortDescription = "Learn more about our blog",
                    Content = "<h2>Welcome to Our Blog</h2><p>We are passionate about sharing knowledge and experiences in technology, development, and programming.</p><p>Our mission is to provide high-quality content that helps developers grow and learn.</p>",
                    Published = true,
                    CreatedDate = DateTime.Now
                },
                new Page
                {
                    Title = "Contact Us",
                    Slug = "contact",
                    ShortDescription = "Get in touch with us",
                    Content = "<h2>Contact Information</h2><p>Have questions? We'd love to hear from you. Send us a message and we'll respond as soon as possible.</p>",
                    Published = true,
                    CreatedDate = DateTime.Now
                },
                new Page
                {
                    Title = "Privacy Policy",
                    Slug = "privacy-policy",
                    ShortDescription = "Our Privacy Policy",
                    Content = "<h2>Privacy Policy</h2><p>This privacy policy sets out how we use and protect any information that you give us when you use this website.</p>",
                    Published = true,
                    CreatedDate = DateTime.Now
                }
            };

            await _context.Pages!.AddRangeAsync(pages);
        }
    }
}
