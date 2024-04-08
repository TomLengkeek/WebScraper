using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using WebScraper.Database.Entities;

namespace WebScraper.Webservice;

public class ApplicationDbContext : DbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            // Build configuration
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            // Get connection string from appsettings.json
            string connectionString = configuration.GetConnectionString("DatabaseConnectionString");

            // Ignore SSL certificate validation
            optionsBuilder.UseSqlServer(connectionString, options => options.EnableRetryOnFailure());
        }    
    }
    
    public DbSet<Notice>? Notices { get; set; }
}