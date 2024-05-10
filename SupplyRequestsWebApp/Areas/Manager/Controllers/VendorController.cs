using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
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
    public class VendorController : Controller
    {
        private readonly IApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<VendorController> _logger;
        private readonly UserHelper _userHelper;

        public VendorController(IApplicationDbContext context, UserManager<IdentityUser> userManager, ILogger<VendorController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
            _userHelper = new UserHelper(userManager, logger);
        }

        // GET: Vendors
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                // Check if Vendors DbSet is null
                if (_context.Vendors == null)
                {
                    // Log the error
                    _logger.LogError("Entity set 'IApplicationDbContext.Vendors' is null");
                    // Return a Problem response
                    throw new SqlNullValueException("Entity set 'IApplicationDbContext.Vendors' is null");
                }

                // Retrieve all vendors including their creators
                var vendors = await _context.Vendors
                    .Include(v => v.CreatedBy)
                    .ToListAsync();

                // Return the view with the list of vendors
                return View(vendors);
            }
            catch (Exception ex)
            {
                // Log any exceptions that occur
                _logger.LogError(ex, "An error occurred while retrieving vendors");

                // Handle the exception gracefully, perhaps redirecting to an error page
                // or displaying a friendly error message to the user
                return View("Error", new ErrorViewModel(String.Format("An error occurred while retrieving vendors. {0}", ex.Message)));
            }
        }

        // GET: VendorController/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            try
            {
                // Check if id or Vendors DbSet is null
                if (id == null || _context.Vendors == null)
                {
                    // Log the error
                    _logger.LogError("Id is null or DbSet 'Vendors' is null");
                    // Return NotFound result
                    throw new ArgumentNullException(nameof(id));
                }

                // Find the vendor with the provided id including its creator
                var vendor = await _context.Vendors
                    .Include(v => v.CreatedBy)
                    .FirstOrDefaultAsync(v => v.Id == id);

                // If vendor is not found, return NotFound result
                if (vendor == null)
                {
                    throw new KeyNotFoundException(String.Format("Vendor not found with id: {0}", id));
                }

                // Return the view with vendor details
                return View(vendor);
            }
            catch (Exception ex)
            {
                // Log any exceptions that occur
                _logger.LogError(ex, "An error occurred while retrieving vendor details for id: {id}", id);

                // Handle the exception gracefully, perhaps redirecting to an error page
                // or displaying a friendly error message to the user
                return View("Error", new ErrorViewModel(String.Format("An error occurred while vendor details. {0}", ex.Message)));
            }
        }


        // GET: VendorController/Create
        public IActionResult Create()
        {
            try
            {
                // Return the Create view
                return View();
            }
            catch (Exception ex)
            {
                // Log any exceptions that occur
                _logger.LogError(ex, "An error occurred while loading the Create page");

                // Handle the exception gracefully, perhaps redirecting to an error page
                // or displaying a friendly error message to the user
                return View("Error", new ErrorViewModel(String.Format("An error occurred while loading the Create page. {0}", ex.Message)));
            }
        }

        // POST: VendorController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,CreatedBy,CreatedByUserId")] Vendor vendor)
        {
            try
            {
                // Check if Name field is valid
                if (ModelState["Name"]?.ValidationState == ModelValidationState.Valid)
                {
                    // Check if a vendor with the same name already exists
                    if (await _context.Vendors.FirstOrDefaultAsync(v => v.Name == vendor.Name) != null)
                    {
                        ModelState.AddModelError(string.Empty, "Vendor with this name already exists.");
                        return View(vendor);
                    }

                    // Get the current user and user id
                    var (userId, foundUser) = await _userHelper.GetCurrentUserAsync(User).ConfigureAwait(false);

                    // Set the CreatedBy and CreatedByUserId properties of the vendor
                    vendor.CreatedBy = (User)foundUser;
                    vendor.CreatedByUserId = userId;

                    // Add the vendor to the database
                    _context.Add(vendor);
                    await _context.SaveChangesAsync();

                    // Redirect to the Index action
                    return RedirectToAction(nameof(Index));
                }
                // If Name field is not valid, return to the view with the vendor
                return View(vendor);
            }
            catch (Exception ex)
            {
                // Log any exceptions that occur
                _logger.LogError(ex, "An error occurred while creating a new vendor");

                // Handle the exception gracefully, perhaps redirecting to an error page
                // or displaying a friendly error message to the user
                return View("Error", new ErrorViewModel(String.Format("An error occurred while creating a new vendor. {0}", ex.Message)));

            }
        }

        // GET: VendorController/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            try
            {
                // Check if id or Vendors DbSet is null
                if (id == null || _context.Vendors == null)
                {
                    // Log the error
                    _logger.LogError("Id is null or DbSet 'Vendors' is null");
                    // Return NotFound result
                    throw new ArgumentNullException(nameof(id));
                }

                // Find the vendor with the provided id including its creator
                var vendor = await _context.Vendors
                    .Include(v => v.CreatedBy)
                    .FirstOrDefaultAsync(v => v.Id == id);

                // If vendor is not found, return NotFound result
                if (vendor == null)
                {
                    throw new KeyNotFoundException(String.Format("Vendor not found with id: {0}", id));
                }

                // Return the view with the vendor details
                return View(vendor);
            }
            catch (Exception ex)
            {
                // Log any exceptions that occur
                _logger.LogError(ex, "An error occurred while retrieving vendor details for edit with id: {id}", id);

                // Handle the exception gracefully, perhaps redirecting to an error page
                // or displaying a friendly error message to the user
                return View("Error", new ErrorViewModel(String.Format("An error occurred while retrieving vendor details. {0}", ex.Message)));
            }
        }

        // POST: VendorController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,CreatedBy,CreatedByUserId")] Vendor vendor)
        {
            try
            {
                // Check if id matches the vendor id
                if (id != vendor.Id)
                {
                    throw new ArgumentException("Incorrect vendor id.");
                }

                // Check if Name field is valid
                if (ModelState["Name"]?.ValidationState == ModelValidationState.Valid)
                {
                    // Check if a vendor with the same name already exists
                    if (await _context.Vendors.FirstOrDefaultAsync(v => v.Name == vendor.Name && v.Id != vendor.Id) != null)
                    {
                        ModelState.AddModelError(string.Empty, "Vendor with this name already exists.");
                        return View(vendor);
                    }

                    // Find the existing vendor in the database
                    var foundVendor = await _context.Vendors.FirstOrDefaultAsync(v => v.Id == vendor.Id);

                    // If vendor is not found, return NotFound
                    if (foundVendor == null)
                    {
                        throw new KeyNotFoundException(String.Format("Vendor not found with id: {0}", id));
                    }

                    // Update the vendor name
                    foundVendor.Name = vendor.Name;

                    try
                    {
                        // Update the vendor in the database
                        _context.Vendors.Update(foundVendor);
                        await _context.SaveChangesAsync();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        // Check if the vendor still exists
                        if (!VendorExists(vendor.Id))
                        {
                            throw new KeyNotFoundException(String.Format("Vendor not found with id: {0}", id));
                        }
                        else
                        {
                            throw;
                        }
                    }

                    // Redirect to the Index action
                    return RedirectToAction(nameof(Index));
                }

                // If Name field is not valid, return to the view with the vendor
                return View(vendor);
            }
            catch (Exception ex)
            {
                // Log any exceptions that occur
                _logger.LogError(ex, "An error occurred while updating vendor with id: {id}", id);

                // Handle the exception gracefully, perhaps redirecting to an error page
                // or displaying a friendly error message to the user
                return View("Error", new ErrorViewModel(String.Format("An error occurred while updating vendor. {0}", ex.Message)));
            }
        }

        // GET: VendorController/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            try
            {
                // Check if id or Vendors DbSet is null
                if (id == null || _context.Vendors == null)
                {
                    // Log the error
                    _logger.LogError("Id is null or DbSet 'Vendors' is null");
                    // Return NotFound result
                    throw new ArgumentNullException(nameof(id));
                }

                // Find the vendor with the provided id including its creator
                var vendor = await _context.Vendors
                    .Include(v => v.CreatedBy)
                    .FirstOrDefaultAsync(m => m.Id == id);

                // If vendor is not found, return NotFound result
                if (vendor == null)
                {
                    throw new KeyNotFoundException(String.Format("Vendor not found with id: {0}", id));
                }

                // Return the view with the vendor details
                return View(vendor);
            }
            catch (Exception ex)
            {
                // Log any exceptions that occur
                _logger.LogError(ex, "An error occurred while retrieving vendor details for deletion with id: {id}", id);

                // Handle the exception gracefully, perhaps redirecting to an error page
                // or displaying a friendly error message to the user
                return View("Error", new ErrorViewModel(String.Format("An error occurred while retrieving vendor details for deletion. {0}", ex.Message)));
            }
        }

        // POST: VendorController/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                // Check if Vendors DbSet is null
                if (_context.Vendors == null)
                {
                    // Log the error
                    _logger.LogError("Vendors DbSet is null");
                    // Return a Problem result
                    throw new SqlNullValueException("Entity set 'IApplicationDbContext.Vendors' is null");
                }

                // Find the vendor with the provided id
                var vendor = await _context.Vendors.Include(v => v.CreatedBy).FirstOrDefaultAsync(v => v.Id == id);

                // If vendor is found, remove it from the context
                if (vendor != null)
                {
                    _context.Vendors.Remove(vendor);

                    try
                    {
                        // Save changes to the database
                        await _context.SaveChangesAsync();
                    }
                    catch (DbUpdateException ex)
                    {
                        // Log any exceptions that occur
                        _logger.LogError(ex, "Vendor already used in some of the supply requests. " +
                            "Firstly, delete all supply requests and items related to this vendor.");

                        ModelState.AddModelError(string.Empty, "Vendor already used in some of the supply requests. " +
                            "Firstly, delete all supply requests and items related to this vendor.");
                        return View(vendor);
                    }
                }

                // Redirect to the Index action
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // Log any exceptions that occur
                _logger.LogError(ex, "An error occurred while deleting vendor with id: {id}", id);

                // Handle the exception gracefully, perhaps redirecting to an error page
                // or displaying a friendly error message to the user
                return View("Error", new ErrorViewModel(String.Format("An error occurred while deleting vendor. {0}", ex.Message)));
            }
        }

        // Check whether Vendor exists or not.
        private bool VendorExists(int id)
        {
            return (_context.Vendors?.Any(v => v.Id == id)).GetValueOrDefault();
        }

    }
}
