using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using WebScraper.Webservice;
using WebScraper.Webservice.Models;
using WebScraper.Webservice.Repositories;
using WebScraper.Webservice.Repositories.Interfaces;
using WebScraper.Webservice.Services;
using WebScraper.Webservice.Services.Interfaces;


try
{
    var configuration = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json")
        .Build();
    
    var connectionString = configuration["DatabaseConnectionString"] ?? throw new Exception("DatabaseConnectionString should be present in appsettings.json");

    // Add dependency injection
    var services = new ServiceCollection();
    
    services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(connectionString));
    
    services.AddTransient<ISearchService, SearchService>();
    services.AddTransient<INoticeRepository, NoticeRepository>();

    services.AddSingleton<IConfiguration>(configuration);
    
    var serviceProvider = services.BuildServiceProvider();
    
    var searchService = serviceProvider.GetRequiredService<ISearchService>();
    var noticeRepository = serviceProvider.GetRequiredService<INoticeRepository>();
    
    var limit = int.Parse(configuration["SearchLimit"] ?? throw new Exception("SearchLimit should be present in appsettings.json"));
    var searchScope = configuration["SearchScope"] ?? throw new Exception("SearchScope should be present in appsettings.json");
    var searchOnlyLatestVersion = configuration["SearchOnlyLatestVersion"] ?? throw new Exception("SearchOnlyLatestVersion should be present in appsettings.json");
    var searchQuery = configuration["searchQuery"] ?? throw new Exception("SearchQuery should be present in appsettings.json");
    
    // If StartDate is not present or is null take the current date as the start date
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
    
    // Start the main loop.
    await RunAsync(startDate, limit, searchScope, searchOnlyLatestVersion, searchQuery, fields, searchService,
        noticeRepository);
}
catch (Exception e)
{
    Console.WriteLine($"Fatal Error: {e.Message}");
}

return;

async Task RunAsync(DateTime date, int limit, string searchScope, string searchOnlyLatestVersion, string searchQuery, string[] fields, ISearchService searchService, INoticeRepository noticeRepository)
{
    var page = 1;

    var andString = !string.IsNullOrEmpty(searchQuery) ? "AND" : "";
    
    while (true)
    {
        var searchRequest = new SearchRequest
        {
            Query = $"publication-date={date:yyyyMMdd} {andString} {searchQuery} SORT BY publication-number DESC",
            Page = page,
            Limit = limit,
            Scope = searchScope,
            OnlyLatestVersions = bool.Parse(searchOnlyLatestVersion),
            Fields = fields
        };

        // Perform the search request to retrieve notices.
        var result = await searchService.SearchAsync(searchRequest);

        if (string.IsNullOrEmpty(result))
        {
            Console.WriteLine($"Error: search request did not return anything for date: {date.Date}");
            date = date.AddDays(-1);
            
            continue;
        }
        
        // Parse the Json result.
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





