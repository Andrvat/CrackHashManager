using Manager.DataContracts.Entities;
using MongoDB.Driver;

namespace Manager.Database;

public interface ICrackHashDbContext
{
    IMongoCollection<CrackHashRequestResultEntity> RequestResults { get; }
    IMongoCollection<CrackHashWorkerTaskEntity> WorkerTasks { get; }
}