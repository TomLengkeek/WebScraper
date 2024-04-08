using WebScraper.Webservice.Models;

namespace WebScraper.Webservice.Services.Interfaces;

public interface ISearchService
{
    Task<string> SearchAsync(SearchRequest searchRequest);
}