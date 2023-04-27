using DataContracts.Enum;
using Manager.DataContracts.Entities;

namespace Manager.Database;

public interface ICrackHashService
{ 
    Task<CrackHashRequestResultEntity?> GetRequestResultByRequestId(string requestId);
    Task AddOrUpdateRequestInfo(CrackHashRequestResultEntity entity);
    Task UpdateRequestProcessingStatus(string requestId, RequestProcessingStatus status);
    Task UpdateRequestData(string requestId, List<string> data);
    Task AddOrUpdateWorkerTask(CrackHashWorkerTaskEntity entity);
    Task<List<CrackHashWorkerTaskEntity>> GetWorkerTasksByRequestId(string requestId);
}