using Microsoft.EntityFrameworkCore;

namespace Application.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<AppUser> AppUsers { get; set; }
        public DbSet<AppResource> AppResources { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AppUser>(entity =>
            {
                entity.ToTable("AppUsers");
                entity.HasKey(e => e.Id);
                entity.Property(x => x.Username).UseCollation("NOCASE");
                entity.HasIndex(x => x.Username).IsUnique();
            });

            modelBuilder.Entity<AppResource>(entity =>
            {
                entity.ToTable("AppResources");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Type).UseCollation("NOCASE");
                entity.Property(x => x.Code).UseCollation("NOCASE");
                entity.Property(x => x.Culture).UseCollation("NOCASE");
                entity.HasIndex(x => new { x.Code, x.Culture }).IsUnique();
            });
        }
    }
}

