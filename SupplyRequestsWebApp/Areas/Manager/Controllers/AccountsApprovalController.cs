﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SupplyManagement.WebApp.Data;
using SupplyManagement.WebApp.Models.Enums;
using SupplyManagement.WebApp.Models.ViewModels;

namespace SupplyManagement.WebApp.Areas.Manager.Controllers
{
	[Area("Manager")]
	[Authorize(Policy = "Manager")]
	public class AccountsApprovalController : Controller
	{
		private readonly ApplicationDbContext _context;
		private readonly UserManager<IdentityUser> _userManager;
		private readonly ILogger<AccountsApprovalController> _logger;

		public AccountsApprovalController(ApplicationDbContext context, UserManager<IdentityUser> userManager, ILogger<AccountsApprovalController>? logger)
		{
			_context = context;
			_userManager = userManager;
			_logger = logger;
		}

		// GET: Account Approval Requests
		[HttpGet]
		public async Task<IActionResult> Index()
		{
			try
			{
				// Logging information
				_logger.LogInformation("Retrieving account approval requests.");

				// Null check for ApplicationDbContext
				if (_context.Users == null)
				{
					_logger.LogError("Entity set 'ApplicationDbContext.Users' is null");
					return Problem("Entity set 'ApplicationDbContext.Users' is null");
				}

				// Retrieve users with Created account status
				var users = await _context.Users
					.Where(u => u.AccountStatus == AccountStatuses.Created)
					.Select(user => new ApproveAccountViewModel()
					{
						Id = user.Id,
						Name = user.Name,
						Surname = user.Surname,
						Email = user.Email,
						// Null check for UserManager
						Roles = String.Join(',', _userManager.GetRolesAsync(user).Result.ToArray() ?? new string[0]),
						AccountStatus = user.AccountStatus,
					})
					.ToListAsync();

				// Logging information
				_logger.LogInformation("Successfully retrieved account approval requests.");

				return View(users);
			}
			catch (Exception ex)
			{
				// Logging error
				_logger.LogError(ex, "An error occurred while retrieving account approval requests.");
				return Problem("An error occurred while retrieving account approval requests.", statusCode: 500);
			}
		}


		// GET: AccountsApprovalController/Edit/5
		public async Task<IActionResult> Edit(string? id)
		{
			try
			{
				// Logging information
				_logger.LogInformation($"Editing account with ID: {id}");

				// Check for null ID or Users context
				if (id == null || _context.Users == null)
				{
					_logger.LogError("ID or Users context is null");
					return NotFound();
				}

				// Retrieve user by ID
				var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);

				// Check if user exists
				if (user == null)
				{
					_logger.LogError($"User with ID '{id}' not found.");
					return NotFound();
				}

				// Create view model for editing
				var approveAccountViewModel = new ApproveAccountViewModel()
				{
					Id = user.Id,
					Name = user.Name,
					Surname = user.Surname,
					Email = user.Email,
					// Null check for UserManager
					Roles = String.Join(',', _userManager.GetRolesAsync(user).Result?.ToArray() ?? new string[0]),
					AccountStatus = user.AccountStatus,
				};

				// Retrieve account statuses for dropdown
				var accountStatuses = from AccountStatuses status in Enum.GetValues(typeof(AccountStatuses))
									  where status != AccountStatuses.Created
									  select new { Id = (int)status, Name = status.ToString() };
				ViewData["accountStatuses"] = new SelectList(accountStatuses, "Id", "Name");

				// Logging information
				_logger.LogInformation($"Account with ID: {id} successfully retrieved for editing.");

				return View(approveAccountViewModel);
			}
			catch (Exception ex)
			{
				// Logging error
				_logger.LogError(ex, "An error occurred while editing the account.");
				return Problem("An error occurred while editing the account.", statusCode: 500);
			}
		}

		// POST: AccountsApprovalController/Edit/5
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(string id, ApproveAccountViewModel approveAccountViewModel)
		{
			try
			{
				// Logging information
				_logger.LogInformation($"Updating account with ID: {id}");

				// Validate ID
				if (id != approveAccountViewModel.Id)
				{
					_logger.LogError($"ID mismatch: Provided ID: {id}, ViewModel ID: {approveAccountViewModel.Id}");
					return NotFound();
				}

				// Retrieve user by ID
				var user = _context.Users.FirstOrDefault(u => u.Id == id);

				// Check if user exists
				if (user == null)
				{
					_logger.LogError($"User with ID '{id}' not found.");
					return NotFound();
				}

				// Update account status
				if (ModelState.IsValid)
				{
					user.AccountStatus = approveAccountViewModel.AccountStatus;
					_context.Update(user);
					await _context.SaveChangesAsync();
					// Logging information
					_logger.LogInformation($"Account with ID: {id} updated successfully.");
					return RedirectToAction(nameof(Index));
				}

				// Retrieve account statuses for dropdown
				var accountStatuses = from AccountStatuses status in Enum.GetValues(typeof(AccountStatuses))
									  where status != AccountStatuses.Created
									  select new { Id = (int)status, Name = status.ToString() };
				ViewData["accountStatuses"] = new SelectList(accountStatuses, "Id", "Name");

				return View(user);
			}
			catch (Exception ex)
			{
				// Logging error
				_logger.LogError(ex, "An error occurred while updating the account.");
				return Problem("An error occurred while updating the account.", statusCode: 500);
			}
		}

		// Checks whether user exists or not.
		private bool UserExists(string id)
		{
			return (_context.Users?.Any(v => v.Id == id)).GetValueOrDefault();
		}
	}
}
