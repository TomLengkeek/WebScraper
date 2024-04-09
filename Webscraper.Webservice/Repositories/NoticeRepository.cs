using System.Globalization;
using Newtonsoft.Json.Linq;
using WebScraper.Webservice.Entities;
using WebScraper.Webservice.Repositories.Interfaces;
using WebScraper.Webservice.Services;

namespace WebScraper.Webservice.Repositories;

public class NoticeRepository : INoticeRepository
{
    private readonly ApplicationDbContext _dbContext;
    
    public NoticeRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task SaveNotice(JToken notice)
    {
        var officialLanguageArray = notice["official-language"]?.Values<string>();

        var officialLanguage = officialLanguageArray?.FirstOrDefault();
        
        if (officialLanguage == null)
        {
            Console.WriteLine("Error: saving notice failed - no official language");
            return;
        }

        var parsingService = new ParsingService(officialLanguage, notice);

        // Get the result notice and check the data.
        var resultNotice = ExtractData(parsingService);

        if (resultNotice == null)
            return;
        
        // Duplicate check.
        if (_dbContext.Notices.Any((n) => n.PublicationNumber == resultNotice.PublicationNumber))
        {
            Console.WriteLine("Error: Notice already saved in database");
            return;
        }

        _dbContext.Notices.Add(resultNotice);

        await _dbContext.SaveChangesAsync();
        
        Console.WriteLine($"Succesfully saved notice: {resultNotice.Id}"); 
    }

    private Notice? ExtractData(ParsingService parsingService)
    {
        var publicationNumber = parsingService.GetValueFromToken("publication-number");

        if (string.IsNullOrEmpty(publicationNumber))
        {
            Console.WriteLine("Error: saving notice failed - no publication number");
            return null;
        }
        
        var title = parsingService.GetValueFromToken( "notice-title");

        if (string.IsNullOrEmpty(title))
        {
            Console.WriteLine("Error: saving notice failed - no title");
            return null;
        }
        
        var deadline = parsingService.GetValueFromToken( "deadline-receipt-request");
        
        if (string.IsNullOrEmpty(deadline))
            deadline = DateTime.MinValue.ToString(CultureInfo.InvariantCulture);

        var publicationDate = parsingService.GetValueFromToken( "publication-date");
        
        if (string.IsNullOrEmpty(publicationDate))
            deadline = DateTime.MinValue.ToString(CultureInfo.InvariantCulture);

        var buyerName = parsingService.GetValueFromToken( "buyer-name");
        var buyerCountry = parsingService.GetValueFromToken( "buyer-country");
        var contractNature = parsingService.GetValueFromToken( "contract-nature");
        var type = parsingService.GetValueFromToken( "notice-type");
        var country = parsingService.GetValueFromToken( "place-of-performance");
        var description = parsingService.GetValueFromToken( "description-lot");

        var resultNotice = new Notice()
        {
            PublicationNumber = publicationNumber,
            Title = title,
            Description = description,
            Deadline = DateTime.Parse(deadline),
            PublicationDate = DateTime.Parse(publicationDate),
            BuyerName = buyerName,
            BuyerCountry = buyerCountry,
            ContractNature = contractNature,
            Type = type,
            Country = country
        };

        return resultNotice;
    }
}
