using AspNetCoreHero.ToastNotification.Abstractions;
using Blog.Models;
using Blog.Utilites;
using Blog.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Blog.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class UserController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly INotyfService _notification;
        public UserController(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager
            ,INotyfService notyfService)
        {
            _notification = notyfService;
            _userManager = userManager; 
            _signInManager = signInManager;
        }
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            var vm = users.Select(x =>  new UserVM()
            {
                Id = x.Id,
                FirstName = x.FirstName,
                LastName = x.LastName,
                UserName = x.UserName,
                Email = x.Email,
            }).ToList();
            foreach (var user in vm)
            {
                var singleUser = await _userManager.FindByIdAsync(user.Id);
                var role = await _userManager.GetRolesAsync(singleUser);
                user.Role = role.FirstOrDefault()! ;
            }

            return View(vm);
        }


  
        [Authorize(Roles ="Admin")]
        [HttpGet]
        public async  Task<IActionResult> ResetPassword(string id)
        {
            var existingUser = await _userManager.FindByIdAsync(id);
            if (existingUser == null)
            {
                _notification.Error("User dosenot exist");
                return View();
            }
            var vm = new ResetPasswordVM()
            {
                Id = existingUser.Id,
                UserName= existingUser.UserName
            };
            return View(vm);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordVM vm)
        {
            if (!ModelState.IsValid) { return View(vm); }
            var existinguser = await _userManager.FindByIdAsync(vm.Id);
            if (existinguser == null)
            {
                _notification.Error("User does not exist");
                return View(vm);
            }
            var token = await _userManager.GeneratePasswordResetTokenAsync(existinguser);
            var result = await _userManager.ResetPasswordAsync(existinguser, token, vm.NewPassword);
            if (result.Succeeded)
            {
                _notification.Success("Password Reset Successfully");
                return RedirectToAction(nameof(Index));
            }
            
            foreach (var error in result.Errors)
            {
                _notification.Error(error.Description);
            }
            return View(vm);
        }

        [Authorize(Roles ="Admin")]
        [HttpGet]
        public IActionResult Register()
        {
            return View(new RegisterVM());
        }

         
         [Authorize(Roles ="Admin")]
        [HttpPost]
        public  async Task<IActionResult> Register(RegisterVM vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }
            var checkbyEmail = await _userManager.FindByEmailAsync(vm.Email);
            if (checkbyEmail !=null)
            {
                _notification.Error("Email already exists");
                return View(vm);
            }
            var checkbyUsername = await _userManager.FindByNameAsync(vm.UserName);
            if (checkbyUsername != null)
            {
                _notification.Error("UserName already exists");
                return View(vm);
            }
            var user = new ApplicationUser()
            {
                Email = vm.Email,
                UserName = vm.UserName,
                FirstName = vm.FirstName,
                LastName = vm.LastName,
            };
            var result = await _userManager.CreateAsync(user, vm.Password);
            if (result.Succeeded)
            {
                if (vm.Role == "Admin")
                {
                    await _userManager.AddToRoleAsync(user, WebsiteRoles.WebsiteAdmin);
                }
                else
                {
                    await _userManager.AddToRoleAsync(user, WebsiteRoles.WebsiteAuthor);

                }
                _notification.Success("User registered seccessfully");
                return RedirectToAction("Index","User", new {area="Admin"});
            }
            return View(vm);
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity!.IsAuthenticated)
            {
                return RedirectToAction("Index", "Post", new { area = "Admin" });
            }
            return View(new LoginVM());
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginVM vm)
        {
            if (!ModelState.IsValid)
            {
                _notification.Error("Please fill in all required fields!");
                return View(vm);
            }

            var result = await _signInManager.PasswordSignInAsync(vm.UserName, vm.Password, vm.RememberMe, lockoutOnFailure: true);
            if (result.Succeeded)
            {
                _notification.Success("Login successful");
                return RedirectToAction("Index", "Post", new { area = "Admin" });
            }
            
            if (result.IsLockedOut)
            {
                _notification.Warning("Account locked. Please try again later.");
                return View(vm);
            }
            
            _notification.Error("Invalid login attempt. Please check your username and password.");
            return View(vm);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _notification.Success("You are logged out successfully");
            return RedirectToAction("Login");
        }
        [HttpGet("AccessDenied")]
        [Authorize]
        public IActionResult AccessDenied()
        {
            return RedirectToAction("AccessDenied", "Account", new { area = "" });
        }

    }
}
