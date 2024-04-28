using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SupplyManagement.WebApp.Data;
using SupplyManagement.WebApp.Models;
using SupplyManagement.WebApp.Models.Enums;
using SupplyManagement.WebApp.Models.ViewModels;

namespace SupplyManagement.WebApp.Areas.Admin.Controllers
{
	[Area("Admin")]
    [Authorize(Policy = "Admin")]
    public class ManagerController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<ManagerController> _logger;

        public ManagerController(ApplicationDbContext context, UserManager<IdentityUser> userManager, ILogger<ManagerController>? logger)
        {
            _context = context;
            _userManager = userManager;
			_logger = logger;
        }

		// GET: Managers
		[HttpGet]
		public async Task<IActionResult> Index()
		{
			try
			{
				// Logging information
				_logger.LogInformation("Retrieving managers");

				// Check if the Users DbSet is null
				if (_context.Users == null)
				{
					// Log the error
					_logger.LogError("Entity set 'ApplicationDbContext.Users' is null");
					// Return a problem response
					return Problem("Entity set 'ApplicationDbContext.Users' is null");
				}

				// Retrieve all users in the 'Manager' role
				var userManagers = await _userManager.GetUsersInRoleAsync("Manager").ConfigureAwait(false);

				// Convert users to RegisterViewModels
				var managers = userManagers
					.Select(u => (User)u)
					.Select(u => new RegisterViewModel
					{
						Id = u.Id,
						Username = u.UserName ?? "",
						Email = u.Email ?? "",
						Name = u.Name,
						Surname = u.Surname,
						// Null check for UserManager
						UserRole = String.Join(',', _userManager.GetRolesAsync(u).Result?.ToArray() ?? new string[0])
					})
					.ToList();

				// Logging information
				_logger.LogInformation($"Retrieved {managers.Count} managers successfully");

				// Return the view with the list of managers
				return View(managers);
			}
			catch (Exception ex)
			{
				// Log any exceptions that occur
				_logger.LogError(ex, "An error occurred while retrieving managers");
				// Return an error view or message
				return StatusCode(500, "An error occurred while processing your request");
			}
		}



		// GET: ManagerController/Details/5
		public async Task<IActionResult> Details(string? id)
		{
			try
			{
				// Check if the id is null or Users DbSet is null
				if (id == null || _context.Users == null)
				{
					// Log the error
					_logger.LogError("User id is null or DbSet 'Users' is null");
					// Return a not found response
					return NotFound();
				}

				// Retrieve the manager with the specified id
				var manager = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);

				// If manager is not found, return not found response
				if (manager == null)
				{
					// Log the error
					_logger.LogError("Manager not found with id: {id}", id);
					// Return a not found response
					return NotFound();
				}

				// Create a RegisterViewModel to display manager details
				var managerView = new RegisterViewModel
				{
					Id = manager.Id,
					Username = manager.UserName ?? "",
					Email = manager.Email ?? "",
					Name = manager.Name,
					Surname = manager.Surname,
					// Null check for UserManager
					UserRole = String.Join(',', _userManager.GetRolesAsync(manager).Result?.ToArray() ?? new string[0])
				};

				// Log information
				_logger.LogInformation("Retrieved information for manager with id: {id}", id);

				// Return the view with manager details
				return View(managerView);
			}
			catch (Exception ex)
			{
				// Log any exceptions that occur
				_logger.LogError(ex, "An error occurred while retrieving manager details for id: {id}", id);
				// Return an error view or message
				return StatusCode(500, "An error occurred while processing your request");
			}
		}



		// GET: ManagerController/Create
		public IActionResult Create()
		{
			try
			{
				// Logging information
				_logger.LogInformation("Loading create manager page");

				// Return the create view
				return View();
			}
			catch (Exception ex)
			{
				// Log any exceptions that occur
				_logger.LogError(ex, "An error occurred while loading create manager page");
				// Return an error view or message
				return StatusCode(500, "An error occurred while processing your request");
			}
		}

		// POST: ManagerController/Create
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(RegisterViewModel model)
		{
			try
			{
				// Check if the ModelState is valid
				if (ModelState.IsValid)
				{
					// Check if a user with the same email already exists
					var existingUser = await _userManager.FindByEmailAsync(model.Email);
					if (existingUser != null)
					{
						// Log the error
						_logger.LogError("Account with email {email} already exists", model.Email);
						// Add error message to ModelState
						ModelState.AddModelError(string.Empty, "Account with this email already exists.");
						// Return to the view with the model
						return View(model);
					}

					// Create a new User object with the provided model data
					var user = new User
					{
						UserName = model.Username,
						Email = model.Email,
						Name = model.Name,
						Surname = model.Surname,
						AccountStatus = AccountStatuses.Approved,
					};

					// Attempt to create the user
					var result = await _userManager.CreateAsync(user, model.Password);

					// If user creation is successful
					if (result.Succeeded)
					{
						_logger.LogInformation("Created user with email: {email}", model.Email);

						// Check if the user role is "Manager"
						if (model.UserRole == "Manager")
						{
							// Add the user to the "Manager" role
							result = await _userManager.AddToRoleAsync(user, model.UserRole);
						}
						else
						{
							// Log the error
							_logger.LogError("Incorrect user role: {role}", model.UserRole);
							// Add error message to ModelState
							ModelState.AddModelError(string.Empty, $"Incorrect user role: {model.UserRole}");
							// Return to the view with the model
							return View(model);
						}

						// If adding user to role is successful, redirect to Manager index
						if (result.Succeeded)
						{
							_logger.LogInformation("User with email: {email} added to the 'Manager' role", model.Email);
							return RedirectToAction("Index", "Manager");
						}
					}

					// If there are errors, add them to the ModelState
					foreach (var error in result.Errors)
					{
						ModelState.AddModelError(error.Code, error.Description);
					}

					// Add a general error message to ModelState
					ModelState.AddModelError(string.Empty, "Invalid");
				}
				// If ModelState is not valid, return to the view with the model
				return View(model);
			}
			catch (Exception ex)
			{
				// Log any exceptions that occur
				_logger.LogError(ex, "An error occurred while creating a manager");
				// Return an error view or message
				return StatusCode(500, "An error occurred while processing your request");
			}
		}


		// GET: ManagerController/Delete/5
		public async Task<IActionResult> Delete(string? id)
		{
			try
			{
				// Check if the id is null or if the Users DbSet is null
				if (id == null || _context.Users == null)
				{
					// Log the error
					_logger.LogError("Id is null or Users DbSet is null");
					// Return NotFound result
					return NotFound();
				}

				// Find the manager with the provided id
				var manager = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);

				// If manager is not found, return NotFound result
				if (manager == null)
				{
					return NotFound();
				}

				// Create a view model for the manager
				var managerView = new RegisterViewModel
				{
					Id = manager.Id,
					Username = manager.UserName ?? "",
					Email = manager.Email ?? "",
					Name = manager.Name,
					Surname = manager.Surname,
					// Null check for UserManager
					UserRole = String.Join(',', _userManager.GetRolesAsync(manager).Result?.ToArray() ?? new string[0])
				};

				// Return the view with the manager view model
				return View(managerView);
			}
			catch (Exception ex)
			{
				// Log any exceptions that occur
				_logger.LogError(ex, "An error occurred while deleting a manager");
				// Return an error view or message
				return StatusCode(500, "An error occurred while processing your request");
			}
		}

		// POST: ManagerController/Delete/5
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(string id)
		{
			try
			{
				// Check if the Users DbSet is null
				if (_context.Users == null)
				{
					// Log the error
					_logger.LogError("Users DbSet is null");
					// Return a Problem result
					return Problem("Entity set 'ApplicationDbContext.Managers' is null.");
				}

				// Find the user with the provided id
				var user = await _context.Users.FindAsync(id);

				// If user is found, remove it from the context
				if (user != null)
				{
					_context.Users.Remove(user);
				}

				// Save changes to the database
				await _context.SaveChangesAsync();

				// Log information
				_logger.LogInformation("Deleted manager with id: {id}", id);

				// Redirect to the Index action
				return RedirectToAction(nameof(Index));
			}
			catch (Exception ex)
			{
				// Log any exceptions that occur
				_logger.LogError(ex, "An error occurred while deleting a manager");
				// Return an error view or message
				return StatusCode(500, "An error occurred while processing your request");
			}
		}
	}
}
