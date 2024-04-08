using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using WebScraper.Database;
using WebScraper.Database.Repositories;
using WebScraper.Webservice;
using WebScraper.Webservice.Models;
using WebScraper.Webservice.Repositories;
using WebScraper.Webservice.Services;

try
{
    #region Configuration

    var configuration = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json")
        .Build();
    
    var connectionString = configuration["DatabaseConnectionString"] ?? throw new Exception("DatabaseConnectionString should be present in appsettings.json");
    
    var services = new ServiceCollection();
    services.AddDbContext<ApplicationDbContext>(builder => builder.UseSqlServer(connectionString));
    
    var serviceProvider = services.BuildServiceProvider();

    var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
    optionsBuilder.UseSqlServer(connectionString);

    await using var dbContext = new ApplicationDbContext();

    var noticeRepository = new NoticeRepository(serviceProvider.GetRequiredService<ApplicationDbContext>());

    var searchService = new SearchService(configuration);
    
    var limit = int.Parse(configuration["SearchLimit"] ?? throw new Exception("SearchLimit should be present in appsettings.json"));
    var searchScope = configuration["SearchScope"] ?? throw new Exception("SearchScope should be present in appsettings.json");
    var searchOnlyLatestVersion = configuration["SearchOnlyLatestVersion"] ?? throw new Exception("SearchOnlyLatestVersion should be present in appsettings.json");
    
    //If StartDate is not present or is null take the current date as the start date
    var startDate = string.IsNullOrEmpty(configuration["StartDate"])
        ? DateTime.UtcNow
        : DateTime.Parse(configuration["StartDate"]);
    
    var fields = configuration.GetSection("SearchFields").Get<List<Fields>>();

    if (fields == null || fields.Count <= 0)
    {
        throw new Exception("SearchFields should be present in appsettings.json");
    }
    
    //We need the official language for parsing
    if (fields.All(f => f.RequestName != "official-language"))
        fields.Add(new Fields()
        {
            RequestName = "official-language"
        });
    
    var requestFields = fields.Select((f) => f.RequestName).ToArray();
    
    var page = 1;
    var date = startDate;

    #endregion
    
    
    //Main loop of the webscraper

    #region Main

    while (true)
    {
        var searchRequest = new SearchRequest
        {
            Query = $"publication-date={date.ToString("yyyyMMdd")} SORT BY publication-number DESC",
            Page = page,
            Limit = limit,
            Scope = searchScope,
            OnlyLatestVersions = bool.Parse(searchOnlyLatestVersion),
            Fields = requestFields,
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
            noticeRepository.SaveNotice(notice, fields);
        }

        var totalNoticeCountToken = jsonObject["totalNoticeCount"];

        var noticesLeft = 0;
        
        if (totalNoticeCountToken == null)
        {
            Console.WriteLine("Error: Could not find totalPageCount using 0 instead");

            noticesLeft = 0;
        }
        else
        {
            noticesLeft = totalNoticeCountToken.Value<int>() - limit;
        }
        
        //logic for moving through pages and dates
        if (noticesLeft <= 0)
        {
            date = date.AddDays(-1);
            page = 1;
        }
        else
            page++;
        
    }

    #endregion
    
}
catch (Exception e)
{
    Console.WriteLine($"Fatal Error: {e.Message}");
}





