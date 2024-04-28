using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SupplyManagement.WebApp.Data;
using SupplyManagement.WebApp.Models;

namespace SupplyManagement.WebApp.Areas.Manager.Controllers
{
	[Area("Manager")]
    [Authorize(Policy = "Manager")]
    public class ItemController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<ItemController> _logger;

        public ItemController(ApplicationDbContext context, UserManager<IdentityUser> userManager, ILogger<ItemController>? logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

		// GET: items
		[HttpGet]
		public async Task<IActionResult> Index()
		{
			try
			{
				// Check if the Items DbSet is null
				if (_context.Items == null)
				{
					// Log the error
					_logger.LogError("Entity set 'ApplicationDbContext.Items' is null");
					// Return a problem response
					return Problem("Entity set 'ApplicationDbContext.Items' is null");
				}

				// Retrieve all items including their associated vendor and creator
				var items = await _context.Items
					.Include(i => i.Vendor)
					.Include(i => i.CreatedBy)
					.ToListAsync();

				// Return the view with the list of items
				return View(items);
			}
			catch (Exception ex)
			{
				// Log any exceptions that occur
				_logger.LogError(ex, "An error occurred while retrieving items");
				// Return an error view or message
				return StatusCode(500, "An error occurred while processing your request");
			}
		}

		// GET: ItemController/Details/5
		public async Task<IActionResult> Details(int? id)
		{
			try
			{
				// Check if the id is null or if the Items DbSet is null
				if (id == null || _context.Items == null)
				{
					// Log the error
					_logger.LogError("Id is null or DbSet 'Items' is null");
					// Return NotFound result
					return NotFound();
				}

				// Find the item with the provided id including its associated vendor and creator
				var item = await _context.Items
					.Include(i => i.CreatedBy)
					.Include(i => i.Vendor)
					.FirstOrDefaultAsync(i => i.Id == id);

				// If item is not found, return NotFound result
				if (item == null)
				{
					return NotFound();
				}

				// Return the view with the item details
				return View(item);
			}
			catch (Exception ex)
			{
				// Log any exceptions that occur
				_logger.LogError(ex, "An error occurred while retrieving item details for id: {id}", id);
				// Return an error view or message
				return StatusCode(500, "An error occurred while processing your request");
			}
		}

		// GET: ItemController/Create
		public IActionResult Create()
		{
			try
			{
				// Populate the dropdown list with vendors
				ViewData["vendorId"] = new SelectList(_context.Vendors.Distinct().ToList(), "Id", "Name");
				return View();
			}
			catch (Exception ex)
			{
				// Log any exceptions that occur
				_logger.LogError(ex, "An error occurred while retrieving vendors for item creation");
				// Return an error view or message
				return StatusCode(500, "An error occurred while processing your request");
			}
		}

		// POST: ItemController/Create
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create([Bind("Id,Name,Vendor,VendorId,CreatedBy,CreatedByUserId")] Item item)
		{
			try
			{
				// Populate the dropdown list with vendors
				ViewData["vendorId"] = new SelectList(_context.Vendors.Distinct().ToList(), "Id", "Name");

				// Check if the model state is valid for the required fields
				if (ModelState["Name"]?.ValidationState == ModelValidationState.Valid
					&& ModelState["VendorId"]?.ValidationState == ModelValidationState.Valid)
				{
					// Check if an item with the same name for the same vendor already exists
					if (await _context.Items.FirstOrDefaultAsync(i => i.Name == item.Name && i.VendorId == item.VendorId) != null)
					{
						ModelState.AddModelError(string.Empty, "Item with this name for this vendor already exists.");
						return View(item);
					}

					// Get the current user's id
					System.Security.Claims.ClaimsPrincipal currentUser = User;
					var userId = _userManager.GetUserId(currentUser);

					// Find the current user
					var foundUser = await _userManager.FindByIdAsync(userId);

					// If the current user is not found, return NotFound
					if (foundUser == null)
					{
						return NotFound("Current authenticated user not found to create a new item");
					}

					// Assign the current user as the creator of the item
					item.CreatedBy = (User)foundUser;
					item.CreatedByUserId = userId;

					// Find the vendor associated with the item
					item.Vendor = await _context.Vendors.FirstOrDefaultAsync(v => v.Id == item.VendorId);

					// Add the item to the context and save changes
					_context.Add(item);
					await _context.SaveChangesAsync();

					// Redirect to the Index action
					return RedirectToAction(nameof(Index));
				}

				// If model state is not valid, return to the view with the item
				return View(item);
			}
			catch (Exception ex)
			{
				// Log any exceptions that occur
				_logger.LogError(ex, "An error occurred while creating a new item");
				// Return an error view or message
				return StatusCode(500, "An error occurred while processing your request");
			}
		}

		// GET: ItemController/Edit/5
		public async Task<IActionResult> Edit(int? id)
		{
			try
			{
				// Check if id or Items DbSet is null
				if (id == null || _context.Items == null)
				{
					// Log the error
					_logger.LogError("Id is null or DbSet 'Items' is null");
					// Return NotFound result
					return NotFound();
				}

				// Find the item with the provided id including its associated vendor and creator
				var item = await _context.Items
					.Include(i => i.Vendor)
					.Include(i => i.CreatedBy)
					.FirstOrDefaultAsync(i => i.Id == id);

				// If item is not found, return NotFound result
				if (item == null)
				{
					return NotFound();
				}

				// Populate the dropdown list with vendors
				ViewData["vendorId"] = new SelectList(_context.Vendors.Distinct().ToList(), "Id", "Name");

				// Return the view with the item details
				return View(item);
			}
			catch (Exception ex)
			{
				// Log any exceptions that occur
				_logger.LogError(ex, "An error occurred while retrieving item details for edit with id: {id}", id);
				// Return an error view or message
				return StatusCode(500, "An error occurred while processing your request");
			}
		}

		// POST: ItemController/Edit/5
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Vendor,VendorId,CreatedBy,CreatedByUserId")] Item item)
		{
			try
			{
				// Populate the dropdown list with vendors
				ViewData["vendorId"] = new SelectList(_context.Vendors.Distinct().ToList(), "Id", "Name");

				// Check if id matches item id
				if (id != item.Id)
				{
					return NotFound();
				}

				// Check if model state is valid for required fields
				if (ModelState["Name"]?.ValidationState == ModelValidationState.Valid
					&& ModelState["VendorId"]?.ValidationState == ModelValidationState.Valid)
				{
					// Find the vendor associated with the item
					var vendor = await _context.Vendors.FirstOrDefaultAsync(i => i.Id == item.VendorId);

					// Check if an item with the same name for the same vendor already exists
					if (await _context.Items.Include(i => i.Vendor).FirstOrDefaultAsync(i => i.Name == item.Name && i.Vendor.Name == vendor.Name) != null)
					{
						ModelState.AddModelError(string.Empty, "Item with this name for this vendor already exists.");
						return View(item);
					}

					// Find the item to update
					var foundItem = await _context.Items.FirstOrDefaultAsync(i => i.Id == item.Id);

					// If item is not found, return NotFound
					if (foundItem == null)
					{
						return NotFound("Item was not found to update");
					}

					// Update item properties
					foundItem.Name = item.Name;
					foundItem.Vendor = vendor;

					// Update item in the database
					_context.Items.Update(foundItem);
					await _context.SaveChangesAsync();

					// Redirect to the Index action
					return RedirectToAction(nameof(Index));
				}

				// If model state is not valid, return to the view with the item
				return View(item);
			}
			catch (Exception ex)
			{
				// Log any exceptions that occur
				_logger.LogError(ex, "An error occurred while updating item with id: {id}", id);
				// Return an error view or message
				return StatusCode(500, "An error occurred while processing your request");
			}
		}

		// GET: ItemController/Delete/5
		public async Task<IActionResult> Delete(int? id)
		{
			try
			{
				// Check if id or Items DbSet is null
				if (id == null || _context.Items == null)
				{
					// Log the error
					_logger.LogError("Id is null or DbSet 'Items' is null");
					// Return NotFound result
					return NotFound();
				}

				// Find the item with the provided id including its associated vendor and creator
				var item = await _context.Items
					.Include(i => i.Vendor)
					.Include(i => i.CreatedBy)
					.FirstOrDefaultAsync(m => m.Id == id);

				// If item is not found, return NotFound result
				if (item == null)
				{
					return NotFound();
				}

				// Return the view with the item details
				return View(item);
			}
			catch (Exception ex)
			{
				// Log any exceptions that occur
				_logger.LogError(ex, "An error occurred while retrieving item details for deletion with id: {id}", id);
				// Return an error view or message
				return StatusCode(500, "An error occurred while processing your request");
			}
		}

		// POST: ItemController/Delete/5
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(int id)
		{
			try
			{
				// Check if Items DbSet is null
				if (_context.Items == null)
				{
					// Log the error
					_logger.LogError("Entity set 'ApplicationDbContext.Items' is null");
					// Return a Problem result
					return Problem("Entity set 'ApplicationDbContext.Items' is null.");
				}

				// Find the item with the provided id
				var item = await _context.Items.FindAsync(id);

				// If item is found, remove it from the context
				if (item != null)
				{
					_context.Items.Remove(item);
				}

				// Save changes to the database
				await _context.SaveChangesAsync();

				// Redirect to the Index action
				return RedirectToAction(nameof(Index));
			}
			catch (Exception ex)
			{
				// Log any exceptions that occur
				_logger.LogError(ex, "An error occurred while deleting item with id: {id}", id);
				// Return an error view or message
				return StatusCode(500, "An error occurred while processing your request");
			}
		}

		// Check whether item exists or not
		private bool ItemExists(int id)
		{
			return (_context.Items?.Any(v => v.Id == id)).GetValueOrDefault();
		}
	}
}
