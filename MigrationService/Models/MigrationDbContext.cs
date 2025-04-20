using Microsoft.EntityFrameworkCore;

namespace MigrationService.Models
{
    public class MigrationDbContext : DbContext
    {
        public MigrationDbContext(DbContextOptions<MigrationDbContext> options) : base(options) { }

        public DbSet<Migrant> Migrants { get; set; }
        public DbSet<Officer> Officers { get; set; }
        public DbSet<Application> Applications { get; set; }
        public DbSet<Document> Documents { get; set; }
        public DbSet<StatusChange> StatusChanges { get; set; }
        public DbSet<Country> Countries { get; set; }
        public DbSet<Language> Languages { get; set; }
        public DbSet<MigrantLanguage> MigrantLanguages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Таблицы
            modelBuilder.Entity<Migrant>().ToTable("Migrants");
            modelBuilder.Entity<Officer>().ToTable("Officers");
            modelBuilder.Entity<Application>().ToTable("Applications");
            modelBuilder.Entity<Document>().ToTable("Documents");
            modelBuilder.Entity<StatusChange>().ToTable("StatusChanges");
            modelBuilder.Entity<Country>().ToTable("Countries");
            modelBuilder.Entity<Language>().ToTable("Languages");
            modelBuilder.Entity<MigrantLanguage>().ToTable("MigrantLanguages");

            // Настройка составного ключа
            modelBuilder.Entity<MigrantLanguage>()
                .HasKey(ml => new { ml.MigrantID, ml.LanguageID });

            modelBuilder.Entity<MigrantLanguage>()
                .HasOne(ml => ml.Migrant)
                .WithMany(m => m.MigrantLanguages)
                .HasForeignKey(ml => ml.MigrantID);

            modelBuilder.Entity<MigrantLanguage>()
                .HasOne(ml => ml.Language)
                .WithMany(l => l.MigrantLanguages)
                .HasForeignKey(ml => ml.LanguageID);
        }
    }
}