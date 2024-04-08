using Microsoft.EntityFrameworkCore;
using WebScraper.Database.Entities;

namespace WebScraper.Database;

public class ApplicationDbContext : DbContext
{
    public DbSet<Notice> Notices { get; set; }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        
    }
}