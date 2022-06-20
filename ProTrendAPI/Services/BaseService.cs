using Microsoft.Extensions.Options;
using MongoDB.Driver;
using ProTrendAPI.Settings;

namespace ProTrendAPI.Services
{
    public class BaseService
    {
        public readonly IMongoDatabase Database;
        public BaseService(IOptions<DBSettings> settings)
        {
            MongoClient client = new(settings.Value.ConnectionURI);
            Database = client.GetDatabase(settings.Value.DatabaseName);
        }
    }
}
