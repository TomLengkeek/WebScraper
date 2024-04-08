﻿using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WebScraper.Webservice.Models;
using WebScraper.Webservice.Services;

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

var searchService = new SearchService(configuration);

try
{
    var limit = configuration["SearchLimit"] ?? throw new Exception("SearchLimit should be present in appsettings.json");
    var scope = configuration["SearchScope"] ?? throw new Exception("SearchScope should be present in appsettings.json");
    
    var fields = configuration.GetSection("SearchFields").Get<List<Fields>>();

    if (fields == null || fields.Count <= 0)
    {
        throw new Exception("SearchFields should be present in appsettings.json");
    }
    
    var requestFields = fields.Select((f) => f.RequestName).ToArray();

    var page = 0;
    //Main loop of the webscraper
    while (true)
    {
        //If StartDate is not present or is null take the current date as the start date
        var startDate = string.IsNullOrEmpty(configuration["StartDate"])
            ? DateTime.UtcNow
            : DateTime.Parse(configuration["StartDate"]);
    
        var searchRequest = new SearchRequest
        {
            Query = $"publication-date={startDate.Year}{startDate.Month}{startDate.Date} SORT BY publication-number DESC",
            Page = 1,
            Limit = int.Parse(limit),
            Scope = scope,
            OnlyLatestVersions = true,
            Fields = requestFields.ToArray(),
        };

        var result = await searchService.SearchAsync(searchRequest);

        if(string.IsNullOrEmpty(result))
            Console.WriteLine("Error: search request did not return anything");
    
        var JsonObject = JToken.Parse(result);
    }
}
catch (Exception e)
{
    Console.WriteLine($"Fatal Error: {e.Message}");
}



