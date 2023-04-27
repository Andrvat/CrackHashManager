using Manager.DataContracts.Entities;
using MongoDB.Driver;

namespace Manager.Database;

public interface ICrackHashDbContext
{
    IMongoCollection<CrackHashRequestResultEntity> RequestResults { get; }
    IMongoCollection<CrackHashWorkerRequestProcessingStatusEntity> WorkerRequestProcessingStatuses { get; }
}