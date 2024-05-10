using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SupplyManagement.Helpers;
using SupplyManagement.Models;
using SupplyManagement.Models.Interfaces;
using SupplyManagement.Models.ViewModels;
using System.Data.SqlTypes;

namespace SupplyManagement.WebApp.Areas.Manager.Controllers
{
    [Area("Manager")]
    [Authorize(Policy = "Manager")]
    public class ItemController : Controller
    {
        private readonly IApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<ItemController> _logger;
        private readonly UserHelper _userHelper;

        public ItemController(IApplicationDbContext context, UserManager<IdentityUser> userManager, ILogger<ItemController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
            _userHelper = new UserHelper(userManager, logger);
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
                    _logger.LogError("Entity set 'IApplicationDbContext.Items' is null");
                    // Return a problem response
                    throw new SqlNullValueException("Entity set 'IApplicationDbContext.Items' is null");
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

                // Handle the exception gracefully, perhaps redirecting to an error page
                // or displaying a friendly error message to the user
                return View("Error", new ErrorViewModel(String.Format("An error occurred while retrieving items. {0}", ex.Message)));
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
                    throw new ArgumentNullException(nameof(id));
                }

                // Find the item with the provided id including its associated vendor and creator
                var item = await _context.Items
                    .Include(i => i.CreatedBy)
                    .Include(i => i.Vendor)
                    .FirstOrDefaultAsync(i => i.Id == id);

                // If item is not found, return NotFound result
                if (item == null)
                {
                    throw new KeyNotFoundException(String.Format("Item not found with id: {0}", id));
                }

                // Return the view with the item details
                return View(item);
            }
            catch (Exception ex)
            {
                // Log any exceptions that occur
                _logger.LogError(ex, "An error occurred while retrieving item details for id: {id}", id);

                // Handle the exception gracefully, perhaps redirecting to an error page
                // or displaying a friendly error message to the user
                return View("Error", new ErrorViewModel(String.Format("An error occurred while retrieving item details. {0}", ex.Message)));
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

                // Handle the exception gracefully, perhaps redirecting to an error page
                // or displaying a friendly error message to the user
                return View("Error", new ErrorViewModel(String.Format("An error occurred while displaying item creation view. {0}", ex.Message)));
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

                    // Get the current user and user id
                    var (userId, foundUser) = await _userHelper.GetCurrentUserAsync(User).ConfigureAwait(false);

                    // Assign the current user as the creator of the item
                    item.CreatedBy = (User)foundUser;
                    item.CreatedByUserId = userId;

                    // Find the vendor associated with the item
                    item.Vendor = await _context.Vendors.FirstOrDefaultAsync(v => v.Id == item.VendorId)
                        ?? throw new KeyNotFoundException(String.Format("Vendor is provided id not found for the item."));

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

                // Handle the exception gracefully, perhaps redirecting to an error page
                // or displaying a friendly error message to the user
                return View("Error", new ErrorViewModel(String.Format("An error occurred while creating item. {0}", ex.Message)));
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
                    throw new ArgumentNullException(nameof(id));
                }

                // Find the item with the provided id including its associated vendor and creator
                var item = await _context.Items
                    .Include(i => i.Vendor)
                    .Include(i => i.CreatedBy)
                    .FirstOrDefaultAsync(i => i.Id == id);

                // If item is not found, return NotFound result
                if (item == null)
                {
                    throw new KeyNotFoundException(String.Format("Item not found with id: {0}", id));
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

                // Handle the exception gracefully, perhaps redirecting to an error page
                // or displaying a friendly error message to the user
                return View("Error", new ErrorViewModel(String.Format("An error occurred while retrieving item details for edit. {0}", ex.Message)));
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
                    throw new ArgumentException("Incorrect item id.");
                }

                // Check if model state is valid for required fields
                if (ModelState["Name"]?.ValidationState == ModelValidationState.Valid
                    && ModelState["VendorId"]?.ValidationState == ModelValidationState.Valid)
                {
                    // Find the vendor associated with the item
                    var vendor = await _context.Vendors.FirstOrDefaultAsync(i => i.Id == item.VendorId)
                        ?? throw new KeyNotFoundException(String.Format("Vendor is provided id not found for the item."));

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
                        throw new KeyNotFoundException(String.Format("Item not found with id: {0}", id));
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

                // Handle the exception gracefully, perhaps redirecting to an error page
                // or displaying a friendly error message to the user
                return View("Error", new ErrorViewModel(String.Format("An error occurred while updating item. {0}", ex.Message)));
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
                    throw new ArgumentNullException(nameof(id));
                }

                // Find the item with the provided id including its associated vendor and creator
                var item = await _context.Items
                    .Include(i => i.Vendor)
                    .Include(i => i.CreatedBy)
                    .FirstOrDefaultAsync(m => m.Id == id);

                // If item is not found, return NotFound result
                if (item == null)
                {
                    throw new KeyNotFoundException(String.Format("Item not found with id: {0}", id));
                }

                // Return the view with the item details
                return View(item);
            }
            catch (Exception ex)
            {
                // Log any exceptions that occur
                _logger.LogError(ex, "An error occurred while retrieving item details for deletion with id: {id}", id);

                // Handle the exception gracefully, perhaps redirecting to an error page
                // or displaying a friendly error message to the user
                return View("Error", new ErrorViewModel(String.Format("An error occurred while retrieving item details for deletion. {0}", ex.Message)));
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
                    _logger.LogError("Entity set 'IApplicationDbContext.Items' is null");
                    // Return a Problem result
                    throw new SqlNullValueException("Entity set 'IApplicationDbContext.Items' is null");
                }

                // Find the item with the provided id
                var item = await _context.Items.Include(i => i.Vendor).FirstOrDefaultAsync(i => i.Id == id);

                // If item is found, remove it from the context
                if (item != null)
                {
                    _context.Items.Remove(item);
                }

                try
                {
                    // Save changes to the database
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException ex)
                {
                    // Log any exceptions that occur
                    _logger.LogError(ex, "Item already used in some of the supply requests. " +
                                    "Firstly, delete all supply requests related to this item.");

                    ModelState.AddModelError(string.Empty, "Item already used in some of the supply requests. " +
                                    "Firstly, delete all supply requests related to this item.");
                    return View(item);
                }

                // Redirect to the Index action
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // Log any exceptions that occur
                _logger.LogError(ex, "An error occurred while deleting item with id: {id}", id);

                // Handle the exception gracefully, perhaps redirecting to an error page
                // or displaying a friendly error message to the user
                return View("Error", new ErrorViewModel(String.Format("An error occurred while deleting item. {0}", ex.Message)));
            }
        }

        // Check whether item exists or not
        private bool ItemExists(int id)
        {
            return (_context.Items?.Any(v => v.Id == id)).GetValueOrDefault();
        }
    }
}
