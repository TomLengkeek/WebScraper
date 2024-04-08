using Microsoft.EntityFrameworkCore;
using WebScraper.Database.Entities;

namespace WebScraper.Database;

public class ApplicationDbContext : DbContext
{
    
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
    public DbSet<Notice> Notices { get; set; }
}