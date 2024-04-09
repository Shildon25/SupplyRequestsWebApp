using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SupplyManagement.WebApp.Data;
using SupplyManagement.WebApp.Models;
using SupplyManagement.WebApp.Models.ViewModels;

namespace SupplyManagement.WebApp.Areas.Customer.Controllers
{
	[Area("Customer")]
	public class SupplyRequestControlller : Controller
	{
		private readonly ApplicationDbContext _context;
		private readonly UserManager<IdentityUser> _userManager;

		public SupplyRequestControlller(ApplicationDbContext context, UserManager<IdentityUser> userManager)
		{
			_context = context;
			_userManager = userManager;
		}

		// GET: items
		[HttpGet]
		public async Task<IActionResult> Index()
		{
			if (_context.SupplyRequests == null)
			{
				return Problem("Entity set 'ApplicationDbContext.SupplyRequests' is null");
			}

			System.Security.Claims.ClaimsPrincipal currentUser = User;
			var userId = _userManager.GetUserId(currentUser); // Get user id:

			var requests = await _context.SupplyRequests
				.Include(r => r.Items)
				.Include(r => r.CreatedBy)
				.Include(r => r.ApprovedBy)
				.Include(r => r.DeliveredBy)
				.Where(r => r.CreatedBy.Id == userId)
				.ToListAsync();

			return View(requests);
		}

		// GET: SupplyRequestControlller/Details/5
		public async Task<IActionResult> Details(int? id)
		{
			if (id == null || _context.SupplyRequests == null)
			{
				return NotFound();
			}
			var item = await _context.SupplyRequests
				.Include(r => r.Items)
				.Include(r => r.CreatedBy)
				.Include(r => r.ApprovedBy)
				.Include(r => r.DeliveredBy)
				.FirstOrDefaultAsync(i => i.Id == id);

			if (item == null)
			{
				return NotFound();
			}

			return View(item);
		}

		// GET: SupplyRequestControlller/Create
		public async Task<IActionResult> Create()
		{
			SupplyRequestViewModel model = new SupplyRequestViewModel();
			model.SelectedItems = await _context.Items.Include(i => i.Vendor)
					.Select(i => new SelectListItem
					{
						Text = String.Format("Item: {item}; Vendor {vendor}", i.Name, i.Vendor.Name),
						Value = i.Id.ToString()
					}).ToListAsync();

			return View(model);
		}

		// POST: SupplyRequestControlller/Create
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(SupplyRequestViewModel model)
		{
			List<Item> items = new List<Item>();

			if (model.Id > 0)
			{
				var request = await _context.SupplyRequests.Include(r => r.Items).FirstOrDefaultAsync(r => r.Id == model.Id);

				if (request == null)
				{
					return NotFound();
				}

				request.Items.ForEach(i => items.Add(i));
			}
			return View();
		}

		// GET: SupplyRequestControlller/Edit/5
		public async Task<IActionResult> Edit(int? id)
		{
			if (id == null || _context.Items == null)
			{
				return NotFound();
			}

			var item = await _context.Items
				.Include(i => i.Vendor)
				.Include(i => i.CreatedBy)
				.FirstOrDefaultAsync(i => i.Id == id);
			if (item == null)
			{
				return NotFound();
			}

			ViewData["vendorId"] = new SelectList(_context.Vendors.Distinct().ToList(), "Id", "Name");
			return View(item);
		}

		// POST: SupplyRequestControlller/Edit/5
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Vendor,VendorId,CreatedBy,CreatedByUserId")] Item item)
		{
			ViewData["vendorId"] = new SelectList(_context.Vendors.Distinct().ToList(), "Id", "Name");
			if (id != item.Id)
			{
				return NotFound();
			}

			if (ModelState["Name"]?.ValidationState == ModelValidationState.Valid
				&& ModelState["VendorId"]?.ValidationState == ModelValidationState.Valid)
			{
				var vendor = await _context.Vendors.FirstOrDefaultAsync(i => i.Id == item.VendorId);

				if (await _context.Items.Include(i => i.Vendor).FirstOrDefaultAsync(i => i.Name == item.Name && i.Vendor.Name == vendor.Name) != null)
				{
					ModelState.AddModelError(string.Empty, "Item with this name for this vendor already exists.");
					return View(item);
				}

				var foundItem = await _context.Items.FirstOrDefaultAsync(i => i.Id == item.Id);

				if (foundItem == null)
				{
					return NotFound("Item was not found to update");
				}

				foundItem.Name = item.Name;
				foundItem.Vendor = vendor;

				try
				{
					_context.Items.Update(foundItem);
					await _context.SaveChangesAsync();
				}
				catch (DbUpdateConcurrencyException)
				{
					if (!ItemExists(item.Id))
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
			return View(item);
		}

		// GET: SupplyRequestControlller/Delete/5
		public async Task<IActionResult> Delete(int? id)
		{
			if (id == null || _context.Items == null)
			{
				return NotFound();
			}

			var item = await _context.Items
				.Include(i => i.Vendor)
				.Include(i => i.CreatedBy)
				.FirstOrDefaultAsync(m => m.Id == id);
			if (item == null)
			{
				return NotFound();
			}

			return View(item);
		}

		// POST: SupplyRequestControlller/Delete/5
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(int id)
		{
			if (_context.Items == null)
			{
				return Problem("Entity set 'ApplicationDbContext.items' is null.");
			}
			var item = await _context.Items.FindAsync(id);
			if (item != null)
			{
				_context.Items.Remove(item);
			}

			await _context.SaveChangesAsync();
			return RedirectToAction(nameof(Index));
		}

		private bool ItemExists(int id)
		{
			return (_context.Items?.Any(v => v.Id == id)).GetValueOrDefault();
		}
	}
}
