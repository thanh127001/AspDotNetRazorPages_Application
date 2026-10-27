using Microsoft.EntityFrameworkCore;

namespace Application.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<AppSetting> AppSettings { get; set; }
        public DbSet<AppUser> AppUsers { get; set; }
        public DbSet<AppContent> AppContents { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AppSetting>(entity =>
            {
                entity.ToTable("AppSettings");
                entity.HasKey(e => e.Id);
                entity.HasIndex(x => x.Code).IsUnique();
                entity.HasIndex(x => x.Name).IsUnique();
            });

            modelBuilder.Entity<AppUser>(entity =>
            {
                entity.ToTable("AppUsers");
                entity.HasKey(e => e.Id);
                entity.HasIndex(x => x.Username).IsUnique();
            });

            modelBuilder.Entity<AppContent>(entity =>
            {
                entity.ToTable("AppContents");
                entity.HasKey(x => x.Id);
                entity.HasIndex(x => x.Code).IsUnique();
            });
        }
    }
}

