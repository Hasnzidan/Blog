using Blog.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Blog.Data
{
    public class DbSeeder
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<DbSeeder> _logger;

        public DbSeeder(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, ILogger<DbSeeder> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task SeedAsync()
        {
            try
            {
                await _context.Database.MigrateAsync();

                if (_context.Categories != null && !await _context.Categories.AnyAsync())
                {
                    _logger.LogInformation("Seeding categories...");
                    await SeedCategoriesAsync();
                }

                if (_context.Posts != null && !await _context.Posts.AnyAsync())
                {
                    _logger.LogInformation("Seeding posts...");
                    await SeedPostsAsync();
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("Database seeding completed successfully");
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
                    NameAr = "تكنولوجيا",
                    NameEn = "Technology",
                    Slug = "technology",
                    Description = "Technology articles"
                },
                new Category
                {
                    NameAr = "برمجة",
                    NameEn = "Programming",
                    Slug = "programming",
                    Description = "Programming articles"
                },
                new Category
                {
                    NameAr = "تطوير الذات",
                    NameEn = "Self Development",
                    Slug = "self-development",
                    Description = "Self development articles"
                },
                new Category
                {
                    NameAr = "ريادة الأعمال",
                    NameEn = "Entrepreneurship",
                    Slug = "entrepreneurship",
                    Description = "Entrepreneurship articles"
                },
                new Category
                {
                    NameAr = "علوم",
                    NameEn = "Science",
                    Slug = "science",
                    Description = "Science articles"
                }
            };

            await _context.Categories!.AddRangeAsync(categories);
            await _context.SaveChangesAsync();
        }

        private async Task SeedPostsAsync()
        {
            if (_context.Categories == null) return;
            
            var author = new ApplicationUser
            {
                UserName = "author",
                Email = "author@example.com",
                FirstName = "أحمد",
                LastName = "محمد",
                EmailConfirmed = true
            };

            var existingAuthor = await _userManager.FindByEmailAsync(author.Email);
            if (existingAuthor == null)
            {
                var result = await _userManager.CreateAsync(author, "Author@123");
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(author, "Author");
                }
            }

            var posts = new List<Post>
            {
                new Post
                {
                    TitleAr = "مقدمة في الذكاء الاصطناعي",
                    TitleEn = "Introduction to AI",
                    ShortDescriptionAr = "مقدمة في الذكاء الاصطناعي وتطبيقاته",
                    ShortDescriptionEn = "Introduction to AI and its applications",
                    DescriptionAr = "الذكاء الاصطناعي هو أحد أهم التقنيات التي تشكل مستقبلنا. في هذا المقال، سنتعرف على المفاهيم الأساسية للذكاء الاصطناعي وكيف يؤثر على حياتنا اليومية...",
                    DescriptionEn = "Artificial Intelligence is one of the most important technologies shaping our future. In this article, we'll learn about the basic concepts of AI and how it affects our daily lives...",
                    Slug = "introduction-to-ai",
                    CategoryId = (await _context.Categories.FirstOrDefaultAsync(c => c.Slug == "technology"))?.Id ?? 1,
                    ApplicationUserId = author.Id,
                    ThumbnailUrl = "/images/blog/ai.jpg"
                },
                new Post
                {
                    TitleAr = "تعلم البرمجة بلغة Python",
                    TitleEn = "Learn Python Programming",
                    ShortDescriptionAr = "دليل المبتدئين لتعلم Python",
                    ShortDescriptionEn = "Beginner's guide to Python",
                    DescriptionAr = "Python هي واحدة من أكثر لغات البرمجة شعبية وسهولة في التعلم. في هذا المقال، سنتعرف على أساسيات البرمجة بلغة Python...",
                    DescriptionEn = "Python is one of the most popular and easy-to-learn programming languages. In this article, we'll explore the basics of Python programming...",
                    Slug = "learn-python-programming",
                    CategoryId = (await _context.Categories.FirstOrDefaultAsync(c => c.Slug == "programming"))?.Id ?? 2,
                    ApplicationUserId = author.Id,
                    ThumbnailUrl = "/images/blog/python.jpg"
                },
                new Post
                {
                    TitleAr = "كيف تبدأ مشروعك الخاص",
                    TitleEn = "How to Start Your Business",
                    ShortDescriptionAr = "خطوات بدء مشروعك الخاص",
                    ShortDescriptionEn = "Steps to start your own business",
                    DescriptionAr = "بدء مشروع خاص يحتاج إلى تخطيط جيد وفهم عميق للسوق. في هذا المقال، سنستعرض الخطوات الأساسية لبدء مشروعك الخاص...",
                    DescriptionEn = "Starting a business requires good planning and deep market understanding. In this article, we'll review the essential steps to start your own business...",
                    Slug = "start-your-business",
                    CategoryId = (await _context.Categories.FirstOrDefaultAsync(c => c.Slug == "entrepreneurship"))?.Id ?? 3,
                    ApplicationUserId = author.Id,
                    ThumbnailUrl = "/images/blog/business.jpg"
                },
                new Post
                {
                    TitleAr = "رحلة تطوير الذات",
                    TitleEn = "Self Development Journey",
                    ShortDescriptionAr = "خطوات تطوير الذات",
                    ShortDescriptionEn = "Steps for self-development",
                    DescriptionAr = "تطوير الذات هو رحلة مستمرة نحو النجاح. في هذا المقال، سنتعرف على أهم الاستراتيجيات لتطوير الذات...",
                    DescriptionEn = "Self-development is a continuous journey towards success. In this article, we'll learn about the most important self-development strategies...",
                    Slug = "self-development-journey",
                    CategoryId = (await _context.Categories.FirstOrDefaultAsync(c => c.Slug == "self-development"))?.Id ?? 4,
                    ApplicationUserId = author.Id,
                    ThumbnailUrl = "/images/blog/self-development.jpg"
                }
            };

            await _context.Posts!.AddRangeAsync(posts);
            await _context.SaveChangesAsync();
        }
    }
}
