using CartService.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace CartService.Infrastructure.Context;

public class CartDbContext : DbContext
{
    public CartDbContext()
    {
    }

    public CartDbContext(DbContextOptions<CartDbContext> options) : base(options)
    {
    }
    
    public virtual DbSet<Cart> Carts { get; set; }
    public virtual DbSet<CartItem> CartItems { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CartDbContext).Assembly);
    }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
      
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())  
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();
            
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            optionsBuilder.UseNpgsql(connectionString);
        }
    }
}