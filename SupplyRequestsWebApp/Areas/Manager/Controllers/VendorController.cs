using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SupplyManagement.WebApp.Data;
using SupplyManagement.WebApp.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace SupplyManagement.WebApp.Areas.Manager.Controllers
{
    [Area("Manager")]
    [Authorize(Policy = "Manager")]
    public class VendorController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public VendorController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Vendors
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (_context.Vendors == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Vendors' is null");
            }

            var vendors = await _context.Vendors
                .Include(v => v.CreatedBy)
                .ToListAsync();

            return View(vendors);
        }

        // GET: VendorController/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Vendors == null)
            {
                return NotFound();
            }
            var vendor = await _context.Vendors
                .Include(v => v.CreatedBy)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (vendor == null)
            {
                return NotFound();
            }

            return View(vendor);
        }

        // GET: VendorController/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: VendorController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,CreatedBy,CreatedByUserId")] Vendor vendor)
        {
            if (ModelState["Name"]?.ValidationState == ModelValidationState.Valid)
            {
                if (await _context.Vendors.FirstOrDefaultAsync(v => v.Name == vendor.Name) != null)
                {
                    ModelState.AddModelError(string.Empty, "Vendor with this name already exists.");
                    return View(vendor);
                }

                try
                {
                    System.Security.Claims.ClaimsPrincipal currentUser = User;
                    var userId = _userManager.GetUserId(currentUser); // Get user id:
                    var foundUser = await _userManager.FindByIdAsync(userId);

                    if (foundUser == null)
                    {
                        return NotFound("Current auth user not found to create a new Vendor");
                    }

                    vendor.CreatedBy = (User)foundUser;
                    vendor.CreatedByUserId = userId;

                    _context.Add(vendor);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }
                catch
                {
                    return View();
                }
            }
            return View(vendor);
        }

        // GET: VendorController/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Vendors == null)
            {
                return NotFound();
            }

            var vendor = await _context.Vendors
                .Include(v => v.CreatedBy)
                .FirstOrDefaultAsync(v => v.Id == id);
            if (vendor == null)
            {
                return NotFound();
            }
            return View(vendor);
        }

        // POST: VendorController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,CreatedBy,CreatedByUserId")] Vendor vendor)
        {
            if (id != vendor.Id)
            {
                return NotFound();
            }

			if (ModelState["Name"]?.ValidationState == ModelValidationState.Valid)
			{
                if (await _context.Vendors.FirstOrDefaultAsync(v => v.Name == vendor.Name) != null)
                {
                    ModelState.AddModelError(string.Empty, "Vendor with this name already exists.");
                    return View(vendor);
                }

                var foundVendor = await _context.Vendors.FirstOrDefaultAsync(v => v.Id == vendor.Id);

				if (foundVendor == null)
				{
					return NotFound("Vendor was not found to update");
				}

				foundVendor.Name = vendor.Name;

				try
                {
                    _context.Vendors.Update(foundVendor);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VendorExists(vendor.Id))
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
            return View(vendor);
        }

        // GET: VendorController/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Vendors == null)
            {
                return NotFound();
            }

            var vendor = await _context.Vendors
                .Include(v => v.CreatedBy)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (vendor == null)
            {
                return NotFound();
            }

            return View(vendor);
        }

        // POST: VendorController/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Vendors == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Vendors' is null.");
            }
            var vendor = await _context.Vendors.FindAsync(id);
            if (vendor != null)
            {
                _context.Vendors.Remove(vendor);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool VendorExists(int id)
        {
            return (_context.Vendors?.Any(v => v.Id == id)).GetValueOrDefault();
        }
    }
}
