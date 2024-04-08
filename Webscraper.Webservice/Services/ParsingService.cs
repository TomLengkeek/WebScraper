using System.Linq.Expressions;
using Newtonsoft.Json.Linq;

namespace WebScraper.Webservice.Services;

public class ParsingService
{
    private readonly string _officialLanguage;
    
    public ParsingService(string officialLanguage)
    {
        _officialLanguage = officialLanguage;
    }
    
    //Get the value from jsonObject using our own custom logic
    public string GetValueFromToken(JToken jsonObject, string propertyName)
    {
        var property = jsonObject[propertyName];
        
        if (property == null)
            return string.Empty;

        var propertyType = property.Type;

        switch (propertyType)
        {
            case JTokenType.Array:
                var value = property.Value<Array>();

                if (value == null || value.Length > 0)
                    return string.Empty;

                return value.GetValue(0)?.ToString() ?? string.Empty;
            
            case JTokenType.Object:
                return GetValueFromToken(property, _officialLanguage.ToLower());

            case JTokenType.String:
                return property.Value<string>() ?? string.Empty;
            
            default:
                return string.Empty;
        }
    }
}