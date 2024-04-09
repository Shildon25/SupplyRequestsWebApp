using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
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

        public ItemController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: items
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (_context.Items == null)
            {
                return Problem("Entity set 'ApplicationDbContext.items' is null");
            }

            var items = await _context.Items.Include(i => i.Vendor)
                .Include(i => i.CreatedBy)
                .Include(I => I.Vendor)
                .ToListAsync();
            
            return View(items);
        }

        // GET: ItemController/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Items == null)
            {
                return NotFound();
            }
            var item = await _context.Items
				.Include(i => i.CreatedBy)
				.Include(I => I.Vendor)
				.FirstOrDefaultAsync(i => i.Id == id);

            if (item == null)
            {
                return NotFound();
            }

            return View(item);
        }

        // GET: ItemController/Create
        public IActionResult Create()
        {
            ViewData["vendorId"] = new SelectList(_context.Vendors.Distinct().ToList(), "Id", "Name");
            return View();
        }

        // POST: ItemController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Vendor,VendorId,CreatedBy,CreatedByUserId")] Item item)
        {
            ViewData["vendorId"] = new SelectList(_context.Vendors.Distinct().ToList(), "Id", "Name");
            if (ModelState["Name"]?.ValidationState == ModelValidationState.Valid
                && ModelState["VendorId"]?.ValidationState == ModelValidationState.Valid)
			{
                if (await _context.Items.FirstOrDefaultAsync(i => i.Name == item.Name && i.VendorId == item.VendorId) != null)
                {
                    ModelState.AddModelError(string.Empty, "Item with this name for this vendor already exists.");
                    return View(item);
                }

                try
                {
                    System.Security.Claims.ClaimsPrincipal currentUser = User;
                    var userId = _userManager.GetUserId(currentUser); // Get user id:
                    var foundUser = await _userManager.FindByIdAsync(userId);

                    if (foundUser == null)
                    {
                        return NotFound("Current auth user not found to create a new item");
                    }

                    item.CreatedBy = (User)foundUser;
                    item.CreatedByUserId = userId;
                    item.Vendor = await _context.Vendors.FirstOrDefaultAsync(v => v.Id == item.VendorId);

                    _context.Add(item);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }
                catch
                {
                    return View();
                }
            }
            return View(item);
        }

        // GET: ItemController/Edit/5
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

        // POST: ItemController/Edit/5
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

        // GET: ItemController/Delete/5
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

        // POST: ItemController/Delete/5
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
