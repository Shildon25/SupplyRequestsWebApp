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

        public ManagerController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Managers
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (_context.Users == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Managers' is null");
            }

            var managers = _userManager
                .GetUsersInRoleAsync("Manager").Result
                .Select(u => (User)u)
                .Select(u => new RegisterViewModel
                {
                    Id = u.Id,
                    Username = u.UserName,
                    Email = u.Email,
                    Name = u.Name,
                    Surname = u.Surname,
                    UserRole = String.Join(',', _userManager.GetRolesAsync(u).Result.ToArray())
                })
                .ToList();

            return View(managers);
        }

        // GET: ManagerController/Details/5
        public async Task<IActionResult> Details(string? id)
        {
            if (id == null || _context.Users == null)
            {
                return NotFound();
            }
            var manager = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id);

            if (manager == null)
            {
                return NotFound();
            }

            var managerView = new RegisterViewModel
            {
                Id = manager.Id,
                Username = manager.UserName,
                Email = manager.Email,
                Name = manager.Name,
                Surname = manager.Surname,
                UserRole = String.Join(',', _userManager.GetRolesAsync(manager).Result.ToArray())
            };

            return View(managerView);
        }

        // GET: ManagerController/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: ManagerController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (await _userManager.FindByEmailAsync(model.Email) != null)
                {
                    ModelState.AddModelError(string.Empty, "Account with this email already exists.");
                    return View(model);
                }

                var user = new User
                {
                    UserName = model.Username,
                    Email = model.Email,
                    Name = model.Name,
                    Surname = model.Surname,
                    AccountStatus = AccountStatuses.Approved,
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    if (model.UserRole == "Manager")
                    {
                        result = await _userManager.AddToRoleAsync(user, model.UserRole);
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, String.Format("Incorrect user role: {role}", model.UserRole));
                        return View(model);
                    }

                    if (result.Succeeded)
                    {
                        return RedirectToAction("Index", "Manager");
                    }
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(error.Code, error.Description);
                }

                ModelState.AddModelError(string.Empty, "Invalid");

            }
            return View(model);
        }

        // GET: ManagerController/Delete/5
        public async Task<IActionResult> Delete(string? id)
        {
            if (id == null || _context.Users == null)
            {
                return NotFound();
            }
            var manager = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id);

            if (manager == null)
            {
                return NotFound();
            }

            var managerView = new RegisterViewModel
            {
                Id = manager.Id,
                Username = manager.UserName,
                Email = manager.Email,
                Name = manager.Name,
                Surname = manager.Surname,
                UserRole = String.Join(',', _userManager.GetRolesAsync(manager).Result.ToArray())
            };

            return View(managerView);
        }

        // POST: ManagerController/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (_context.Users == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Managers' is null.");
            }
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
