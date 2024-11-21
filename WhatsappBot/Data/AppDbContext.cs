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
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<PhoneNumbers> PhoneNumbers { get; set; }
        
    }
};