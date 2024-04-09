using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SupplyManagement.WebApp.Data;
using SupplyManagement.WebApp.Models;
using SupplyManagement.WebApp.Models.ViewModels;
using SupplyManagement.WebApp.Models.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;
using Mono.TextTemplating;

namespace SupplyManagement.WebApp.Areas.Manager.Controllers
{
	[Area("Manager")]
	[Authorize(Policy = "Manager")]
	public class AccountsApprovalController : Controller
	{
		private readonly ApplicationDbContext _context;
		private readonly UserManager<IdentityUser> _userManager;

		public AccountsApprovalController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
		{
			_context = context;
			_userManager = userManager;
		}

		// GET: Account Approval Requests
		[HttpGet]
		public async Task<IActionResult> Index()
		{
			if (_context.Users == null)
			{
				return Problem("Entity set 'ApplicationDbContext.Users' is null");
			}

			var users = await _context.Users.Where(u => u.AccountStatus == AccountStatuses.Created)
				.Select(user => new ApproveAccountViewModel()
				{
					Id = user.Id,
					Name = user.Name,
					Surname = user.Surname,
					Email = user.Email,
					Roles = String.Join(',', _userManager.GetRolesAsync(user).Result.ToArray()),
					AccountStatus = user.AccountStatus,
				})
				.ToListAsync();

			return View(users);
		}

		// GET: AccountsApprovalController/Edit/5
		public async Task<IActionResult> Edit(string? id)
		{
			if (id == null || _context.Users == null)
			{
				return NotFound();
			}

			var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
			var approveAccountViewModel = new ApproveAccountViewModel()
			{
				Id = user.Id,
				Name = user.Name,
				Surname = user.Surname,
				Email = user.Email,
				Roles = String.Join(',', _userManager.GetRolesAsync(user).Result.ToArray()),
				AccountStatus = user.AccountStatus,
			};

			if (user == null)
			{
				return NotFound();
			}
			var accountStatuses = from AccountStatuses status in Enum.GetValues(typeof(AccountStatuses))
							 where status != AccountStatuses.Created
							 select new { Id = (int)status, Name = status.ToString() };
			ViewData["accountStatuses"] = new SelectList(accountStatuses, "Id", "Name");
			return View(approveAccountViewModel);
		}

		// POST: AccountsApprovalController/Edit/5
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(string id, ApproveAccountViewModel approveAccountViewModel)
		{
			if (id != approveAccountViewModel.Id)
			{
				return NotFound();
			}
			var user = _context.Users.FirstOrDefault(u => u.Id == id);

			if (ModelState.IsValid)
			{
				try
				{
					user.AccountStatus = approveAccountViewModel.AccountStatus;
					_context.Update(user);
					await _context.SaveChangesAsync();
				}
				catch (DbUpdateConcurrencyException)
				{
					if (!UserExists(id))
					{
						return NotFound();
					}
					else
					{
						throw;
					}
				}
				return RedirectToAction(nameof(Index));
			}

            var accountStatuses = from AccountStatuses status in Enum.GetValues(typeof(AccountStatuses))
                                  where status != AccountStatuses.Created
                                  select new { Id = (int)status, Name = status.ToString() };
            ViewData["accountStatuses"] = new SelectList(accountStatuses, "Id", "Name");
            return View(user);
		}

		private bool UserExists(string id)
		{
			return (_context.Users?.Any(v => v.Id == id)).GetValueOrDefault();
		}
	}
}
