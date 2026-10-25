using Microsoft.EntityFrameworkCore;

namespace Application.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<AppContent> AppContents { get; set; }
        public DbSet<AppSetting> AppSettings { get; set; }
        public DbSet<AppUser> AppUsers { get; set; }
        public DbSet<AppUserRole> AppUserRoles { get; set; }
        public DbSet<AppUserStatus> AppUserStatuses { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AppContent>(entity =>
            {
                entity.ToTable("AppContents");
                entity.HasKey(x => x.Id);
                entity.HasIndex(x => x.Code).IsUnique();
            });
            modelBuilder.Entity<AppUserRole>(entity =>
            {
                entity.ToTable("AppUserRoles");
                entity.HasKey(x => x.Id);
                entity.HasIndex(x => x.Code).IsUnique();
            });
            modelBuilder.Entity<AppUserStatus>(entity =>
            {
                entity.ToTable("AppUserStatuses");
                entity.HasKey(x => x.Id);
                entity.HasIndex(x => x.Code).IsUnique();
            });

            modelBuilder.Entity<AppSetting>(entity =>
            {
                entity.ToTable("AppSettings");
                entity.HasKey(e => e.Id);
                entity.HasIndex(x => x.Code).IsUnique();
            });

            modelBuilder.Entity<AppUser>(entity =>
            {
                entity.ToTable("AppUsers");
                entity.HasKey(e => e.Id);
                entity.HasIndex(x => x.Code).IsUnique();
            });
        }
    }
}

