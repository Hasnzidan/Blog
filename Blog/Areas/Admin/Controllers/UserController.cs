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
                user.Role = role.FirstOrDefault();
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
            if (!ModelState.IsValid)  { return View(vm); }
            var existinguser = await _userManager.FindByIdAsync(vm.Id);
            if (existinguser == null)
            {
                _notification.Error("User does not exist");
                return View(vm);
            }
            var token = await _userManager.GeneratePasswordResetTokenAsync(existinguser);
            var result = await _userManager.ResetPasswordAsync(existinguser, token, vm.NewPassword);
            if (!result.Succeeded)
            {
                _notification.Success("Password Reset Seccessfully");
               return RedirectToAction(nameof(Index));
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
                if (vm.IsAdmin)
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

        [HttpGet("Login")]
        public IActionResult Login()
        {
            if (!HttpContext.User.Identity.IsAuthenticated)
            {
                return View(new LoginVM());
            }
            return RedirectToAction("Index","User", new {area = "Admin"});
        }

        [HttpPost("Login")]
        public async Task <IActionResult> Login(LoginVM vm) 
        {
           
            if (!ModelState.IsValid) { return View(vm); }
            var existingUser = await _userManager.Users.FirstOrDefaultAsync(x => x.UserName == vm.UserName);
            if (existingUser == null) {
                _notification.Error("Username does not exist");
                return View(vm);
            }
            var verifyPassword = await _userManager.CheckPasswordAsync(existingUser,vm.Password);
            if (!verifyPassword)
            {
                _notification.Error("Password does not match");

                return View(vm);
            }
           await _signInManager.PasswordSignInAsync(vm.UserName, vm.Password, vm.RememberM, true);
            _notification.Success("success Login");
            return RedirectToAction("Index","User",new {area = "Admin"});
        }
        [HttpPost]
        public IActionResult Logout()
        {
            _signInManager.SignOutAsync();
            _notification.Success("You are logged out successfully");
            return RedirectToAction("Index","Home", new {area =""});
        }

    }
}
