using Newtonsoft.Json.Linq;
using WebScraper.Database.Entities;
using WebScraper.Webservice;
using WebScraper.Webservice.Models;

namespace WebScraper.Webservice.Repositories;

public class NoticeRepository
{
    private readonly ApplicationDbContext _dbContext;

    public NoticeRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public void SaveNotice(JToken notice, List<Fields> fields)
    {
        var officialLanguage = notice["official-language"];


        var resultNotice = new Notice();


        Console.WriteLine($"Succesfully saved notice: {resultNotice.Id}"); 
    }
}