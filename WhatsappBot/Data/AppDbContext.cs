using Microsoft.EntityFrameworkCore;
using WhatsappBot.Models;

namespace WhatsappBot.Data
{
    public class AppDbContext(IConfiguration configuration) : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PhoneNumbers>()
                .HasKey(pc => new { pc.Id });
            
            modelBuilder.Entity<PhoneNumbers>()
                .HasIndex(p => p.PhoneNumber)
                .IsUnique();
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<PhoneNumbers> PhoneNumbers { get; set; }
        
    }
};