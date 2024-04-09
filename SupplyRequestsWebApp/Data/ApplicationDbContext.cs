namespace SupplyManagement.WebApp.Data
{
    using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore;
    using SupplyManagement.WebApp.Models;

    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users {  get; set; }
        public DbSet<Vendor> Vendors {  get; set; }
        public DbSet<Item> Items {  get; set; }
        public DbSet<SupplyRequest> SupplyRequests { get; set; }
    }
}
