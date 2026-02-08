using Microsoft.EntityFrameworkCore;
using TestAPI.Domain;
using TestAPI.Domain.Entities;
using TestAPI.Infrastructure.Repositories.SqlServer.Configurations;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace TestAPI.Infrastructure.Repositories.SqlServer
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<WeatherForecast> WeatherForecasts { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>().HasQueryFilter(p => !p.IsDeleted);
            modelBuilder.ApplyConfiguration(new UserConfiguration());

            modelBuilder.Entity<WeatherForecast>().HasQueryFilter(p => !p.IsDeleted);
            modelBuilder.ApplyConfiguration(new WeatherForecastConfiguration());
        }

        /// <summary>
        /// Override SaveChangesAsync to automatically set audit properties
        /// </summary>
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            ApplyAuditInformation();
            return base.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Automatically applies audit information to tracked entities
        /// </summary>
        private void ApplyAuditInformation()
        {
            var entries = ChangeTracker.Entries<DomainEntity>();
            var currentUserId = RequestContext.UserId;
            var utcNow = DateTime.UtcNow;

            foreach (var entry in entries)
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedAt = utcNow;
                        entry.Entity.CreatedBy = currentUserId;
                        entry.Entity.IsDeleted = false;
                        break;

                    case EntityState.Modified:
                        // Don't update CreatedAt and CreatedBy
                        entry.Property(e => e.CreatedAt).IsModified = false;
                        entry.Property(e => e.CreatedBy).IsModified = false;

                        entry.Entity.UpdatedAt = utcNow;
                        entry.Entity.UpdatedBy = currentUserId;
                        break;

                    case EntityState.Deleted:
                        // Implement soft delete
                        entry.State = EntityState.Modified;
                        entry.Entity.IsDeleted = true;
                        entry.Entity.DeletedAt = utcNow;
                        entry.Entity.DeletedBy = currentUserId;
                        break;
                }
            }
        }
    }
}
