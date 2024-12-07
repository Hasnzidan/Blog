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

        private async Task SeedPagesAsync()
        {
            var pages = new List<Page>
            {
                new Page
                {
                    TitleEn = "About Us",
                    TitleAr = "من نحن",
                    Slug = "about",
                    ShortDescriptionEn = "Learn more about our company",
                    ShortDescriptionAr = "تعرف أكثر على شركتنا",
                    ContentEn = "This is the about us page content.",
                    ContentAr = "هذا هو محتوى صفحة من نحن",
                    Published = true,
                    CreatedDate = DateTime.Now
                },
                new Page
                {
                    TitleEn = "Contact Us",
                    TitleAr = "اتصل بنا",
                    Slug = "contact",
                    ShortDescriptionEn = "Get in touch with us",
                    ShortDescriptionAr = "تواصل معنا",
                    ContentEn = "This is the contact us page content.",
                    ContentAr = "هذا هو محتوى صفحة اتصل بنا",
                    Published = true,
                    CreatedDate = DateTime.Now
                },
                new Page
                {
                    TitleEn = "Privacy Policy",
                    TitleAr = "سياسة الخصوصية",
                    Slug = "privacy-policy",
                    ShortDescriptionEn = "Our privacy policy",
                    ShortDescriptionAr = "سياسة الخصوصية لدينا",
                    ContentEn = "This is the privacy policy page content.",
                    ContentAr = "هذا هو محتوى صفحة سياسة الخصوصية",
                    Published = true,
                    CreatedDate = DateTime.Now
                }
            };

            await _context.Pages!.AddRangeAsync(pages);
        }
    }
}
