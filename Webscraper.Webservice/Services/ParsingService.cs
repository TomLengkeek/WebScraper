using System.Linq.Expressions;
using Newtonsoft.Json.Linq;
using WebScraper.Webservice.Services.Interfaces;

namespace WebScraper.Webservice.Services;

public class ParsingService : IParsingService
{
    private readonly string _officialLanguage;
    private readonly JToken _notice;
    
    public ParsingService(string officialLanguage, JToken notice)
    {
        _officialLanguage = officialLanguage;
        _notice = notice;
    }
    
    //Get the value from jsonObject using our own custom logic
    public string GetValueFromToken(string propertyName, JToken? jsonObject = null)
    {
        var property = jsonObject == null ? _notice[propertyName] : jsonObject[propertyName];
        
        if (property == null)
            return string.Empty;

        var propertyType = property.Type;

        switch (propertyType)
        {
            case JTokenType.Array:
                var values = property.Values<string>().ToArray();
                
                return !values.Any() ? string.Empty : values.First();

            case JTokenType.Object:
                return GetValueFromToken(_officialLanguage.ToLower(), property);

            case JTokenType.String:
                return property.Value<string>() ?? string.Empty;
            
            default:
                return string.Empty;
        }
    }
}