using Manager.DataContracts.Entities;
using MongoDB.Driver;

namespace Manager.Database;

public class CrackHashDbContext : ICrackHashDbContext
{
    private readonly IMongoDatabase _db;

    public IMongoCollection<CrackHashRequestResultEntity> RequestResults => _db.GetCollection<CrackHashRequestResultEntity>("RequestResults");
    public IMongoCollection<CrackHashWorkerTaskEntity> WorkerTasks => _db.GetCollection<CrackHashWorkerTaskEntity>("WorkerTasks");

    public CrackHashDbContext(IMongoDbConfig config)
    {
        var client = new MongoClient(config.ConnectionString);
        _db = client.GetDatabase(config.Database);
    }
}