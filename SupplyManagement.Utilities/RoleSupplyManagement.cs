using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity;

namespace SupplyManagement.Utilities
{
    using SupplyManagement.Models;
    using SupplyManagement.Models.Enums;

    public class RoleSupplyManagement: IRoleSupplyManagement
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public RoleSupplyManagement(UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task AddRoleAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            var roles = _roleManager.Roles.Select(role => role.Name);
            if (user != null) 
            {
                await _userManager.AddToRolesAsync(userId, roles.ToArray());
            }
        }

        public Task AddRoleAsync()
        {
            throw new NotImplementedException();
        }

        public async Task CreateRoleAsync()
        {
            throw new NotImplementedException();
        }
    }
}
