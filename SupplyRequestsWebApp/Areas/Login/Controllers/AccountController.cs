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
        private ILogger<AccountController> _logger;

        public AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, ILogger<AccountController>? logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

		[HttpGet]
		[AllowAnonymous]
		public IActionResult Register()
		{
			// Display the registration form
			return View();
		}

		[HttpPost]
		[AllowAnonymous]
		public async Task<IActionResult> Register(RegisterViewModel model)
		{
			// Check if the model state is valid
			if (ModelState.IsValid)
			{
				// Check if a user with the provided email already exists
				if (await _userManager.FindByEmailAsync(model.Email) != null)
				{
					// If user with the email already exists, add an error message and return the registration view
					ModelState.AddModelError(string.Empty, "Account with this email already exists.");
					return View(model);
				}

				// Create a new user object with the provided model data
				var user = new User
				{
					UserName = model.Username,
					Email = model.Email,
					Name = model.Name,
					Surname = model.Surname,
					AccountStatus = AccountStatuses.Created,
				};

				// Attempt to create the user
				var result = await _userManager.CreateAsync(user, model.Password);

				// If user creation is successful
				if (result.Succeeded)
				{
					// Add user to the appropriate role based on the UserRole property in the model
					if (model.UserRole == "Courier" || model.UserRole == "User")
					{
						result = await _userManager.AddToRoleAsync(user, model.UserRole);
					}
					else
					{
						result = await _userManager.AddToRoleAsync(user, "User");
					}

					// If user role assignment is successful, redirect to successful registration page
					if (result.Succeeded)
					{
						// Log successful registration
						_logger.LogInformation("User registered successfully with email: {email}", model.Email);
						return RedirectToAction("SuccessfulRegistration", "Account");
					}
				}

				// If there are errors, add them to the model state
				foreach (var error in result.Errors)
				{
					ModelState.AddModelError(string.Empty, error.Description);
				}

				// Add a general error message to the model state
				ModelState.AddModelError(string.Empty, "Invalid");
			}

			// If model state is not valid, return the registration view with the model
			return View(model);
		}


		[HttpGet]
		[AllowAnonymous]
		public IActionResult SuccessfulRegistration()
		{
			// Display the successful registration page
			return View();
		}

		[HttpGet]
		[AllowAnonymous]
		public IActionResult Login()
		{
			// Display the login page
			return View();
		}

		[HttpPost]
		[AllowAnonymous]
		public async Task<IActionResult> Login(LoginViewModel user)
		{
			// Check if the model state is valid
			if (ModelState.IsValid)
			{
				// Find the user by email
				var foundUser = await _userManager.FindByEmailAsync(user.Email) as User;

				// If user is found and account is approved
				if (foundUser != null && foundUser.AccountStatus == AccountStatuses.Approved)
				{
					// Attempt to sign in the user with the provided credentials
					var result = await _signInManager.PasswordSignInAsync(foundUser, user.Password, true, false);

					// If sign-in is successful, redirect to home page
					if (result.Succeeded)
					{
						// Log successful login
						_logger.LogInformation("User logged in successfully with email: {email}", user.Email);
						return RedirectToAction("Index", "Home");
					}
				}
				else if (foundUser != null && foundUser.AccountStatus == AccountStatuses.Rejected)
				{
					// If account approval request was rejected, add error message and return login page
					ModelState.AddModelError(string.Empty, "Account approval request was rejected by Manager");
				}
				else if (foundUser != null && foundUser.AccountStatus == AccountStatuses.Created)
				{
					// If account is not approved yet, add error message and return login page
					ModelState.AddModelError(string.Empty, "Account not approved");
				}
				else
				{
					// If user not found or invalid credentials, add error message and return login page
					ModelState.AddModelError(string.Empty, "Invalid Credentials");
				}
			}

			// If model state is not valid, return login page with model
			return View(user);
		}

        [HttpGet]
        [Authorize]
        public IActionResult ChangePassword()
        {
            // Display the change password form
            return View();
        }

        [HttpPost]
		[Authorize]
		public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
		{
			// Check if the model state is valid
			if (ModelState.IsValid)
			{
				// Find the current user
				var currentUser = await _userManager.GetUserAsync(User);

				// If current user is found
				if (currentUser != null)
				{
					// Attempt to change the password
					var result = await _userManager.ChangePasswordAsync(currentUser, model.OldPassword, model.NewPassword);

					// If password change is successful
					if (result.Succeeded)
					{
						// Sign out the user to force re-login with the new password
						await _signInManager.SignOutAsync();

						_logger.LogInformation("User changed password successfully.");

						// Redirect to login page
						return RedirectToAction("Login");
					}
					else
					{
						// If password change fails, add error messages to ModelState
						foreach (var error in result.Errors)
						{
							ModelState.AddModelError(string.Empty, error.Description);
						}
					}
				}
				else
				{
					_logger.LogError("Unable to find user for password change.");
					return NotFound();
				}
			}

			// If model state is not valid, return the view with model
			return View(model);
		}


		public async Task<IActionResult> Logout()
		{
			// Sign out the user
			await _signInManager.SignOutAsync();

			// Redirect to the login page
			return RedirectToAction("Login");
		}

	}
}
