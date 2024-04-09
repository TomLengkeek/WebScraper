using Newtonsoft.Json.Linq;

namespace WebScraper.Webservice.Repositories.Interfaces;

public interface INoticeRepository
{
    Task SaveNotice(JToken notice);
}