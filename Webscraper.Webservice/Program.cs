using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices.JavaScript;
using System.Security.Cryptography;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using WebScraper.Webservice.Models;
using WebScraper.Webservice.Services;

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

var limit = configuration["SearchLimit"] ?? throw new Exception("SearchLimit should be present in appsettings.json");
var scope = configuration["SearchScope"] ?? throw new Exception("SearchScope should be present in appsettings.json");
var fields = configuration["SearchFields"] ?? throw new Exception("SearchFields should be present in appsettings.json");

//If StartDate is not present or is null take the current date as the start date
var startData = string.IsNullOrEmpty(configuration["StartDate"])
    ? DateTime.UtcNow
    : DateTime.Parse(configuration["StartDate"]);


var searchService = new SearchService(configuration);

var searchRequest = new SearchRequest
{
    Query = $"publication-date=20231215 SORT BY publication-number DESC",
    Page = 1,
    Limit = limit,
    Scope = scope,
    OnlyLatestVersions = true,
    Fields = fields,
};

var result = await searchService.SearchAsync(searchRequest);

if(string.IsNullOrEmpty(result))
    Console.WriteLine("Error: search request did not return anything");
    
var JsonObject = JToken.Parse(result);

