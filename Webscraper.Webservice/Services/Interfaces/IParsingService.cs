using Newtonsoft.Json.Linq;

namespace WebScraper.Webservice.Services.Interfaces;

public interface IParsingService
{
    string GetValueFromToken(string propertyName, JToken? jsonObject = null);
}