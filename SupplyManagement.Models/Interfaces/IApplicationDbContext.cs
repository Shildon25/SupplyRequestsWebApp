using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace SupplyManagement.Models.Interfaces
{
    public interface IApplicationDbContext
    {
        DbSet<User> Users { get; set; }
        DbSet<Vendor> Vendors { get; set; }
        DbSet<Item> Items { get; set; }
        DbSet<SupplyRequest> SupplyRequests { get; set; }
        DbSet<ItemSupplyRequest> ItemSupplyRequests { get; set; }
        EntityEntry<TEntity> Update<TEntity>(TEntity entity) where TEntity : class;
        EntityEntry<TEntity> Add<TEntity>(TEntity entity) where TEntity : class;
        int SaveChanges();
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default);
        EntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class;
        EntityEntry Entry(object entity);
        void AddRange(params object[] entities);
        void AttachRange(params object[] entities);
        void RemoveRange(params object[] entities);
        void Dispose();
    }
}
