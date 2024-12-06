using AspNetCoreHero.ToastNotification.Abstractions;
using Blog.Models;
using Blog.Utilites;
using Blog.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Blog.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly INotyfService _notification;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            INotyfService notification)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _notification = notification;
        }

        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View(new RegisterVM());
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterVM vm)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = false, message = string.Join(", ", errors) });
                    }

                    foreach (var error in errors)
                    {
                        _notification.Error(error);
                    }
                    return View(vm);
                }

                // Check if email exists
                var checkUserByEmail = await _userManager.FindByEmailAsync(vm.Email);
                if (checkUserByEmail != null)
                {
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = false, message = "Email already exists!" });
                    }
                    _notification.Error("Email already exists!");
                    return View(vm);
                }

                // Check if username exists
                var checkUserByUsername = await _userManager.FindByNameAsync(vm.UserName);
                if (checkUserByUsername != null)
                {
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = false, message = "Username already exists!" });
                    }
                    _notification.Error("Username already exists!");
                    return View(vm);
                }

                // Create new user
                var user = new ApplicationUser
                {
                    UserName = vm.UserName,
                    Email = vm.Email,
                    FirstName = vm.FirstName,
                    LastName = vm.LastName,
                    EmailConfirmed = true // تفعيل البريد الإلكتروني تلقائياً
                };

                var result = await _userManager.CreateAsync(user, vm.Password);

                if (result.Succeeded)
                {
                    // Add user to role
                    await _userManager.AddToRoleAsync(user, vm.Role);

                    // Sign in the user
                    await _signInManager.SignInAsync(user, isPersistent: false);

                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = true, returnUrl = "/" });
                    }

                    _notification.Success("Registration successful! Welcome to our blog.");
                    return RedirectToAction("Index", "Home");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = string.Join(", ", result.Errors.Select(e => e.Description)) });
                }

                foreach (var error in result.Errors)
                {
                    _notification.Error(error.Description);
                }
                return View(vm);
            }
            catch (Exception ex)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "An error occurred during registration." });
                }
                _notification.Error("An error occurred during registration.");
                return View(vm);
            }
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            ViewData["ReturnUrl"] = returnUrl;
            return View(new LoginVM());
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginVM vm, string? returnUrl)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = false, message = string.Join(", ", errors) });
                    }

                    foreach (var error in errors)
                    {
                        _notification.Error(error);
                    }
                    return View(vm);
                }

                // Try to find user by username or email
                var user = await _userManager.FindByNameAsync(vm.UserName) ?? 
                          await _userManager.FindByEmailAsync(vm.UserName);

                if (user == null)
                {
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = false, message = "Invalid username/email or password." });
                    }
                    _notification.Error("Invalid username/email or password.");
                    return View(vm);
                }

                var result = await _signInManager.PasswordSignInAsync(user, vm.Password, vm.RememberMe, false);

                if (result.Succeeded)
                {
                    // Get user's role
                    var roles = await _userManager.GetRolesAsync(user);
                    vm.Role = roles.FirstOrDefault();

                    // تحديد الصفحة التي سيتم التوجيه إليها بناءً على الدور
                    string redirectUrl = returnUrl ?? "/";
                    if (await _userManager.IsInRoleAsync(user, WebsiteRoles.WebsiteAdmin))
                    {
                        redirectUrl = "/Admin/Home";
                    }

                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = true, returnUrl = redirectUrl });
                    }

                    _notification.Success("Login successful! Welcome back.");
                    return LocalRedirect(redirectUrl);
                }
                else
                {
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = false, message = "Invalid username/email or password." });
                    }
                    _notification.Error("Invalid username/email or password.");
                    return View(vm);
                }
            }
            catch (Exception ex)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "An error occurred during login." });
                }
                _notification.Error("An error occurred during login.");
                return View(vm);
            }
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            if (User.Identity!.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordVM vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            var user = await _userManager.FindByEmailAsync(vm.Email);
            if (user == null || !await _userManager.IsEmailConfirmedAsync(user))
            {
                // Don't reveal that the user does not exist or is not confirmed
                _notification.Success("If your email is registered and confirmed, you will receive password reset instructions.");
                return RedirectToAction(nameof(Login));
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var callbackUrl = Url.Action("ResetPassword", "Account",
                new { email = vm.Email, token = token }, protocol: Request.Scheme);

            // TODO: Send email with the callback URL
            // For development, we'll redirect directly
            _notification.Success("Password reset link has been sent to your email.");
            return RedirectToAction(nameof(ResetPassword), new { email = vm.Email, token = token });
        }

        [HttpGet]
        public IActionResult ResetPassword(string email, string token)
        {
            var vm = new ResetPasswordVM
            {
                Email = email,
                Token = token
            };
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordVM vm)
        {
            if (!ModelState.IsValid)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, errors = ModelState.ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).FirstOrDefault()
                    )});
                }
                return View(vm);
            }

            var user = await _userManager.FindByEmailAsync(vm.Email!);
            if (user == null)
            {
                _notification.Error("Invalid request");
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "Invalid request" });
                }
                return RedirectToAction(nameof(Login));
            }

            var result = await _userManager.ResetPasswordAsync(user, vm.Token!, vm.NewPassword!);
            if (result.Succeeded)
            {
                _notification.Success("Password has been reset successfully!");
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = true, returnUrl = Url.Action("Login", "Account") });
                }
                return RedirectToAction(nameof(Login));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = false, message = "Failed to reset password. Please try again." });
            }
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _notification.Success("Logged out successfully!");
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
