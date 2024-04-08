using System.Text.Json.Serialization;

namespace WebScraper.Webservice.Models;

public class SearchRequest
{
    [JsonPropertyName("query")]
    public string Query { get; set; }
    
    [JsonPropertyName("page")]
    public int Page { get; set; }
    
    [JsonPropertyName("limit")]
    public int Limit { get; set; }
    
    [JsonPropertyName("fields")]
    public string[] Fields { get; set; }
    
    [JsonPropertyName("scope")]
    public string Scope { get; set; }
    
    [JsonPropertyName("onlyLatestVersions")]
    public bool OnlyLatestVersions { get; set; }
}