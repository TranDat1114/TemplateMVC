using System.Linq.Expressions;

using Microsoft.Extensions.Configuration;

using Microsoft.EntityFrameworkCore;
using TemplateMVC.Models;
using TemplateMVC.Core.Interface;
using TemplateMVC.Core.Models;

namespace TemplateMVC.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IHttpContextAccessor httpContextAccessor, IConfiguration configuration) : DbContext(options)
    {
        protected readonly IConfiguration _configuration = configuration;

        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

            optionsBuilder.UseSqlServer(_configuration.GetConnectionString("DefaultConnection"));
        }

        public DbSet<Product> Products { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(IBaseEntity).IsAssignableFrom(entityType.ClrType))
                {
                    var parameter = Expression.Parameter(entityType.ClrType, "e");
                    var propertyMethodInfo = typeof(EF).GetMethod("Property")!.MakeGenericMethod(typeof(bool));
                    var isDeletedProperty = Expression.Call(propertyMethodInfo, parameter, Expression.Constant("IsDeleted"));
                    var compareExpression = Expression.Equal(isDeletedProperty, Expression.Constant(false));
                    var lambda = Expression.Lambda(compareExpression, parameter);

                    modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
                }
            }
        }

        public override int SaveChanges()
        {
            SetTimestamps();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SetTimestamps();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void SetTimestamps()
        {
            var currentUser = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "System";

            var entries = ChangeTracker.Entries().Where(x => x.Entity is BaseEntity && (x.State == EntityState.Added || x.State == EntityState.Modified || x.State == EntityState.Deleted));
            // 
            var Now = DateTime.UtcNow;
            foreach (var entry in entries)
            {
                if (entry.Entity is BaseEntity entity)
                {
                    if (entry.State == EntityState.Added)
                    {

                        if (string.IsNullOrEmpty(entity.Guid))
                        {
                            entity.Guid = Guid.NewGuid().ToString();
                        }
                        entity.CreatedAt = Now;
                        entity.CreatedBy = currentUser;
                        entity.IsDeleted = false;
                    }

                    entity.UpdatedAt = Now;
                    entity.UpdatedBy = currentUser;

                    if (entry.State == EntityState.Deleted)
                    {
                        entry.State = EntityState.Modified;
                        entity.IsDeleted = true;
                    }
                }
            }
        }
    }
}
