using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using WebScraper.Webservice.Entities;


namespace WebScraper.Webservice;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        //Logic for ef core to be able to connect to the database
        if (!optionsBuilder.IsConfigured)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            string connectionString = configuration.GetConnectionString("DatabaseConnectionString");
            
            optionsBuilder.UseSqlServer(connectionString, options => options.EnableRetryOnFailure());
        }    
    }
    
    public DbSet<Notice> Notices { get; set; }
}