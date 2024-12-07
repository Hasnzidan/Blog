using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Blog.Controllers
{
    public class LanguageController : Controller
    {
        private readonly ILogger<LanguageController> _logger;

        public LanguageController(ILogger<LanguageController> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        public IActionResult Change(string culture, string returnUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(culture))
                {
                    _logger.LogWarning("Culture parameter was null or empty");
                    return BadRequest("Culture must be specified");
                }

                Response.Cookies.Append(
                    CookieRequestCultureProvider.DefaultCookieName,
                    CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                    new CookieOptions
                    {
                        Expires = DateTimeOffset.UtcNow.AddYears(1),
                        IsEssential = true,
                        SameSite = SameSiteMode.Lax
                    }
                );

                _logger.LogInformation("Language set to: {Culture}", culture);

                return LocalRedirect(returnUrl ?? "~/");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting language culture");
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        public IActionResult SetLanguage(string culture, string returnUrl)
        {
            return Change(culture, returnUrl);
        }
    }
}
