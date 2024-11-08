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
        
        public DbSet<Product> Products { get; set; }
    }
};