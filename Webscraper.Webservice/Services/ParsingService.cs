using Newtonsoft.Json.Linq;

namespace WebScraper.Webservice.Services;

public class ParsingService
{
    public string OfficialLanguage { get; set; }
    
    public string GetValueFromToken(JToken jsonObject, string propertyName)
    {
        var property = jsonObject[propertyName];
        
        if (property == null)
            return string.Empty;
        
        var propertyValue = property.Value<string>();

        if (propertyValue == null)
            return string.Empty;

        var propertyType = property.Type;

        switch (propertyType)
        {
            case JTokenType.Array:
                
                
                break;
        }
    }
}