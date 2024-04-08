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

const int limit = 250;
const string scope = "ACTIVE";

var fields = new [] 
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

var searchService = new SearchService(configuration);

var searchRequest = new SearchRequest
{
    Query = "publication-date=20231215 SORT BY publication-number DESC",
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

