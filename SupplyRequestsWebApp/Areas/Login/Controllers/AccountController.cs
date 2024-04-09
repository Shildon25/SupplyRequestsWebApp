using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SupplyManagement.WebApp.Models;
using SupplyManagement.WebApp.Models.Enums;
using SupplyManagement.WebApp.Models.ViewModels;

namespace SupplyManagement.WebApp.Areas.Login.Controllers
{
    [Area("Login")]
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;

        public AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (await _userManager.FindByEmailAsync(model.Email) != null)
                {
                    ModelState.AddModelError(string.Empty, "Account with this email already exists.");
                    return View(model);
                }

                var user = new User
                {
                    UserName = model.Username,
                    Email = model.Email,
                    Name = model.Name,
                    Surname = model.Surname,
                    AccountStatus = AccountStatuses.Created,
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    if (model.UserRole == "Courier" || model.UserRole == "User")
                    {
                        result = await _userManager.AddToRoleAsync(user, model.UserRole);
                    }
                    else
                    {
                        result = await _userManager.AddToRoleAsync(user, "User");
                    }

                    if (result.Succeeded)
                    {
                        return RedirectToAction("SuccessfulRegistration", "Account");
                    }
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(error.Code, error.Description);
                }

                ModelState.AddModelError(string.Empty, "Invalid");

            }
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult SuccessfulRegistration()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginViewModel user)
        {
            if (ModelState.IsValid)
            {
                User? foundUser = (User?)await _userManager.FindByEmailAsync(user.Email);
                if (foundUser != null && foundUser.AccountStatus == AccountStatuses.Approved)
                {
                    var result = await _signInManager.PasswordSignInAsync(foundUser, user.Password, true, false);

                    if (result.Succeeded)
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }
                else if (foundUser != null && foundUser.AccountStatus == AccountStatuses.Rejected)
                {
                    ModelState.AddModelError(string.Empty, "Account approval request was rejected by Manager");
                }
                else if (foundUser != null && foundUser.AccountStatus == AccountStatuses.Created)
                {
                    ModelState.AddModelError(string.Empty, "Account not approved");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid Credentials");
                }
            }
            return View(user);
        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();

            return RedirectToAction("Login");
        }
    }
}
