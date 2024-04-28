﻿namespace SupplyManagement.WebApp.Data
{
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Hosting;
    using SupplyManagement.WebApp.Models;
    using System.Security.Principal;

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
        public DbSet<ItemSupplyRequest> ItemSupplyRequests { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<ItemSupplyRequest>(entity =>
            {
                entity.HasOne(e => e.Item)
                    .WithMany(e => e.RequestItems)
                    .HasForeignKey(e => e.ItemId)
                    .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(e => e.SupplyRequest)
                    .WithMany(e => e.RequestItems)
                    .HasForeignKey(e => e.SupplyRequestId)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
