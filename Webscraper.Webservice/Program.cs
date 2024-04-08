using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using WebScraper.Webservice;
using WebScraper.Webservice.Models;
using WebScraper.Webservice.Repositories;
using WebScraper.Webservice.Services;


async Task RunAsync(DateTime date, int limit, int page, string searchScope, string searchOnlyLatestVersion, string searchQuery, string[] fields, SearchService searchService, NoticeRepository noticeRepository)
{
    while (true)
    {
        var andString = !string.IsNullOrEmpty(searchQuery) ? "AND" : "";
        
        var searchRequest = new SearchRequest
        {
            Query = $"publication-date={date.ToString("yyyyMMdd")} {andString} {searchQuery} SORT BY publication-number DESC",
            Page = page,
            Limit = limit,
            Scope = searchScope,
            OnlyLatestVersions = bool.Parse(searchOnlyLatestVersion),
            Fields = fields,
        };

        var result = await searchService.SearchAsync(searchRequest);

        if (string.IsNullOrEmpty(result))
        {
            Console.WriteLine($"Error: search request did not return anything for date: {date.Date}");
            date = date.AddDays(-1);
            continue;
        }
        
        var jsonObject = JToken.Parse(result);

        var notices = jsonObject["notices"];

        if (notices == null)
        {
            Console.WriteLine($"Error: search request did not return anything for date: {date.Date}");
            date = date.AddDays(-1);
            continue;
        }

        foreach (var notice in notices)
        {
           await noticeRepository.SaveNotice(notice);
        }

        var totalNoticeCountToken = jsonObject["totalNoticeCount"];

        if (totalNoticeCountToken == null || !int.TryParse(totalNoticeCountToken.ToString(), out int totalNoticeCount))
        {
            Console.WriteLine("Error: Could not retrieve total notice count.");
            break; 
        }
        
        var noticesLeft = totalNoticeCount - (page * limit);

        if (noticesLeft <= 0)
        {
            date = date.AddDays(-1);
            page = 1;
        }
        else
        {
            // Update page
            page++;
        }
    }
}

try
{
    var configuration = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json")
        .Build();
    
    var connectionString = configuration["DatabaseConnectionString"] ?? throw new Exception("DatabaseConnectionString should be present in appsettings.json");

    var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>()
        .UseSqlServer(connectionString)
        .Options;

    await using var dbContext = new ApplicationDbContext(optionsBuilder);

    var noticeRepository = new NoticeRepository(dbContext);

    var searchService = new SearchService(configuration);
    
    var limit = int.Parse(configuration["SearchLimit"] ?? throw new Exception("SearchLimit should be present in appsettings.json"));
    var searchScope = configuration["SearchScope"] ?? throw new Exception("SearchScope should be present in appsettings.json");
    var searchOnlyLatestVersion = configuration["SearchOnlyLatestVersion"] ?? throw new Exception("SearchOnlyLatestVersion should be present in appsettings.json");
    var searchQuery = configuration["searchQuery"] ?? throw new Exception("SearchQuery should be present in appsettings.json");
    
    //If StartDate is not present or is null take the current date as the start date
    var startDate = string.IsNullOrEmpty(configuration["StartDate"])
        ? DateTime.UtcNow
        : DateTime.Parse(configuration["StartDate"]);

    var fields = new[]
    {
        "publication-number",
        "place-of-performance",
        "contract-nature",
        "buyer-name",
        "buyer-country",
        "publication-date",
        "deadline-receipt-request",
        "notice-title",
        "official-language",
        "notice-type",
        "description-lot"
    };
    
    var page = 1;
    
    //Start the main loop
    await RunAsync(startDate, limit, page, searchScope, searchOnlyLatestVersion, searchQuery,fields, searchService,
        noticeRepository);
}
catch (Exception e)
{
    Console.WriteLine($"Fatal Error: {e.Message}");
}





