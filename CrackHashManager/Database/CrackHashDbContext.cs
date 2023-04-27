using DataContracts.Entities;
using Manager.DataContracts.Entities;
using MongoDB.Driver;

namespace Manager.Database;

public class CrackHashDbContext : ICrackHashDbContext
{
    private readonly IMongoDatabase _db;

    public IMongoCollection<CrackHashRequestResultEntity> RequestResults => _db.GetCollection<CrackHashRequestResultEntity>("RequestResults");
    
    public CrackHashDbContext(IMongoDbConfig config)
    {
        var client = new MongoClient(config.ConnectionString);
        _db = client.GetDatabase(config.Database);
    }
}