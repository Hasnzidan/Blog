using Blog.Data;
using Blog.Models;
using Blog.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using CsvHelper;

namespace Blog.Controllers
{
    public class SearchController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SearchController> _logger;
        private const string PreferencesCookieName = "SearchPreferences";

        public SearchController(ApplicationDbContext context, ILogger<SearchController> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IActionResult> Index(SearchViewModel searchModel)
        {
            try
            {
                // Load categories and tags for dropdowns
                searchModel.Categories = await _context.Set<Category>()
                    .Select(c => new CategoryViewModel { Id = c.Id, Name = c.Name })
                    .ToListAsync();

                var tagList = await _context.Tags!
                    .Select(t => new TagViewModel 
                    { 
                        Id = t.Id, 
                        NameEn = t.NameEn,
                        NameAr = t.NameAr 
                    })
                    .ToListAsync();

                searchModel.Tags = tagList;

                // Load user preferences
                if (Request.Cookies.ContainsKey(PreferencesCookieName))
                {
                    var preferences = JsonSerializer.Deserialize<SearchPreferences>(
                        Request.Cookies[PreferencesCookieName]!);
                    if (preferences != null)
                    {
                        searchModel.PreferredCategory = preferences.Category;
                        searchModel.PreferredTags = preferences.Tags;
                    }
                }

                var query = _context.Posts!
                    .Include(p => p.ApplicationUser)
                    .Include(p => p.Category)
                    .Include(p => p.PostTags)
                        .ThenInclude(pt => pt.Tag)
                    .AsQueryable();

                // Apply search filters
                if (!string.IsNullOrEmpty(searchModel.SearchTerm))
                {
                    var searchTerm = searchModel.SearchTerm.ToLower();
                    var currentCulture = CultureInfo.CurrentCulture.Name.StartsWith("ar") ? "ar" : "en";
                    query = query.Where(x =>
                        (currentCulture == "ar" && 
                            (x.TitleAr.ToLower().Contains(searchTerm) || 
                             x.ShortDescriptionAr.ToLower().Contains(searchTerm) || 
                             x.DescriptionAr.ToLower().Contains(searchTerm))) ||
                        (currentCulture == "en" && 
                            (x.TitleEn.ToLower().Contains(searchTerm) || 
                             x.ShortDescriptionEn.ToLower().Contains(searchTerm) || 
                             x.DescriptionEn.ToLower().Contains(searchTerm))) ||
                        x.PostTags!.Any(pt => 
                            (currentCulture == "ar" && pt.Tag!.NameAr.ToLower().Contains(searchTerm)) ||
                            (currentCulture == "en" && pt.Tag!.NameEn.ToLower().Contains(searchTerm))) ||
                        (currentCulture == "ar" && x.Category!.NameAr.ToLower().Contains(searchTerm)) ||
                        (currentCulture == "en" && x.Category!.NameEn.ToLower().Contains(searchTerm))
                    );
                }

                if (!string.IsNullOrEmpty(searchModel.Author))
                {
                    query = query.Where(p => 
                        p.ApplicationUser!.FirstName!.ToLower().Contains(searchModel.Author.ToLower()) ||
                        p.ApplicationUser!.LastName!.ToLower().Contains(searchModel.Author.ToLower()));
                }

                if (searchModel.CategoryId.HasValue)
                {
                    query = query.Where(p => p.CategoryId == searchModel.CategoryId);
                }

                if (searchModel.SelectedTags.Any())
                {
                    query = query.Where(p => p.PostTags.Any(pt => searchModel.SelectedTags.Contains(pt.TagId)));
                }

                if (searchModel.FromDate.HasValue)
                {
                    query = query.Where(p => p.CreateDate >= searchModel.FromDate.Value);
                }

                if (searchModel.ToDate.HasValue)
                {
                    query = query.Where(p => p.CreateDate <= searchModel.ToDate.Value);
                }

                // Apply sorting
                query = searchModel.SortBy?.ToLower() switch
                {
                    "date_desc" => query.OrderByDescending(p => p.CreateDate),
                    "date_asc" => query.OrderBy(p => p.CreateDate),
                    "title_asc" => query.OrderBy(p => p.TitleEn),
                    "title_desc" => query.OrderByDescending(p => p.TitleEn),
                    _ => query.OrderByDescending(p => p.CreateDate)
                };

                // Get total count for pagination
                searchModel.TotalPosts = await query.CountAsync();

                // Apply pagination and get posts
                var posts = await query
                    .Skip((searchModel.Page - 1) * searchModel.PageSize)
                    .Take(searchModel.PageSize)
                    .Select(p => new PostViewModel
                    {
                        Id = p.Id,
                        Title = p.TitleEn,
                        ShortDescription = p.ShortDescriptionEn,
                        Author = p.ApplicationUser!.FirstName + " " + p.ApplicationUser.LastName,
                        CreateDate = p.CreateDate,
                        ThumbnailUrl = p.ThumbnailUrl,
                        Slug = p.Slug,
                        Category = p.Category!.Name,
                        Tags = p.PostTags.Select(pt => pt.Tag.NameEn).ToList(),
                        Description = p.DescriptionEn
                    })
                    .ToListAsync();

                // Apply highlighting after getting the data
                if (!string.IsNullOrEmpty(searchModel.SearchTerm))
                {
                    foreach (var post in posts)
                    {
                        post.HighlightedContent = HighlightSearchTerm(post.Description ?? "", searchModel.SearchTerm);
                    }
                }

                searchModel.Posts = posts;

                // Save preferences if requested
                if (searchModel.SavePreferences)
                {
                    var preferences = new SearchPreferences
                    {
                        Category = searchModel.CategoryId.HasValue ? 
                            searchModel.Categories.First(c => c.Id == searchModel.CategoryId).Name : null,
                        Tags = searchModel.Tags
                            .Where(t => searchModel.SelectedTags.Contains(t.Id))
                            .Select(t => t.NameEn)
                            .ToList()
                    };

                    Response.Cookies.Append(PreferencesCookieName, 
                        JsonSerializer.Serialize(preferences),
                        new CookieOptions { Expires = DateTime.Now.AddYears(1) });
                }

                _logger.LogInformation("Search performed for term: {SearchTerm}, found {Count} results", searchModel.SearchTerm, posts.Count);
                
                return View(searchModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while searching for term: {SearchTerm}", searchModel.SearchTerm);
                TempData["Error"] = "An error occurred while performing the search.";
                return View(searchModel);
            }
        }

        [HttpGet]
        public async Task<IActionResult> AutoComplete(string term)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(term))
                {
                    return Json(new string[] { });
                }

                var currentCulture = CultureInfo.CurrentCulture.Name.StartsWith("ar") ? "ar" : "en";
                
                var suggestions = await _context.Posts
                    .Where(p => (currentCulture == "ar" && p.TitleAr.Contains(term)) ||
                               (currentCulture == "en" && p.TitleEn.Contains(term)))
                    .Select(p => currentCulture == "ar" ? p.TitleAr : p.TitleEn)
                    .Take(10)
                    .ToListAsync();

                _logger.LogInformation("Autocomplete performed with term: {Term}, Results count: {Count}", 
                    term, suggestions.Count);

                return Json(suggestions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during autocomplete with term: {Term}", term);
                return Json(new string[] { });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Export(SearchViewModel searchModel)
        {
            try
            {
                var csvContent = "Title,Author,Category,Tags,Date\n";
                foreach (var post in searchModel.Posts)
                {
                    csvContent += $"{post.Title},{post.Author},{post.Category},{string.Join(";", post.Tags)},{post.CreateDate:yyyy-MM-dd}\n";
                }

                byte[] bytes = System.Text.Encoding.UTF8.GetBytes(csvContent);
                return File(bytes, "text/csv", "search_results.csv");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while exporting search results");
                TempData["Error"] = "An error occurred while exporting the search results.";
                return RedirectToAction("Index", searchModel);
            }
        }

        [HttpGet]
        public async Task<IActionResult> ExportToCsv(string query)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query))
                {
                    return BadRequest("Search query is required");
                }

                var currentCulture = CultureInfo.CurrentCulture.Name.StartsWith("ar") ? "ar" : "en";
                
                var searchResults = await _context.Posts
                    .Include(p => p.Category)
                    .Where(p => (currentCulture == "ar" && 
                               (p.TitleAr.Contains(query) || 
                                p.ShortDescriptionAr.Contains(query))) ||
                              (currentCulture == "en" && 
                               (p.TitleEn.Contains(query) || 
                                p.ShortDescriptionEn.Contains(query))))
                    .Select(p => new
                    {
                        Title = currentCulture == "ar" ? p.TitleAr : p.TitleEn,
                        ShortDescription = currentCulture == "ar" ? p.ShortDescriptionAr : p.ShortDescriptionEn,
                        Category = currentCulture == "ar" ? p.Category.NameAr : p.Category.NameEn,
                        CreateDate = p.CreateDate
                    })
                    .ToListAsync();

                var memoryStream = new MemoryStream();
                var streamWriter = new StreamWriter(memoryStream, Encoding.UTF8);
                var csvWriter = new CsvWriter(streamWriter, CultureInfo.InvariantCulture);

                await csvWriter.WriteRecordsAsync(searchResults);
                await streamWriter.FlushAsync();
                memoryStream.Position = 0;

                _logger.LogInformation("CSV export performed with query: {Query}, Results count: {Count}", 
                    query, searchResults.Count);

                return File(memoryStream, "text/csv", $"search_results_{DateTime.Now:yyyyMMddHHmmss}.csv");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during CSV export with query: {Query}", query);
                return StatusCode(500, "An error occurred while exporting the search results");
            }
        }

        private static string HighlightSearchTerm(string content, string? searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm)) return content;

            var regex = new Regex(searchTerm, RegexOptions.IgnoreCase);
            return regex.Replace(content, match => $"<mark>{match.Value}</mark>");
        }
    }

    public class SearchPreferences
    {
        public string? Category { get; set; }
        public List<string> Tags { get; set; } = new();
    }
}
