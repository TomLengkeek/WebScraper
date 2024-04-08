using System.Text;
using System.Text.Json;
using WebScraper.Webservice.Models;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;

namespace WebScraper.Webservice.Services;

public class SearchService
{
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;
    private readonly string _endpointUri;

    public SearchService(IConfiguration configuration) 
    {
        _configuration = configuration;
        
        _httpClient = new HttpClient();
        
        _endpointUri = configuration["EndpointUri"] ?? throw new Exception("Default endpoint should be present in appsettings.json");
    }

    public async Task<string> SearchAsync(SearchRequest searchRequest)
    {
        try
        {
            var body = new StringContent(JsonSerializer.Serialize(searchRequest), Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync(new Uri(_endpointUri), body);
            
            var result = await response.Content.ReadAsStringAsync();
            
            if (response.IsSuccessStatusCode)
            {
                return result;
            }

            throw new Exception($"Error occurred: {result}");
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error: {e.Message}");
            
            return string.Empty;
        }
    }
}