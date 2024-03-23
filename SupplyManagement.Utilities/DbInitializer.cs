namespace SupplyManagement.Utilities
{
    using SupplyManagement.Models;
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;

    public class DbInitializer: IDbInitializer
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public void CreateRoles()
        {
        }

        public void CreateSuperAdmin() 
        {
        }
    }
}
