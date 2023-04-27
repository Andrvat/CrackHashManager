using DataContracts.Enum;
using Manager.DataContracts.Entities;
using MongoDB.Driver;

namespace Manager.Database;

public class CrackHashService : ICrackHashService
{
    private readonly ICrackHashDbContext _dbContext;

    public CrackHashService(ICrackHashDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task<CrackHashRequestResultEntity?> GetRequestResultByRequestId(string requestId)
    {
        var filterDefinition = Builders<CrackHashRequestResultEntity>.Filter.Eq(e => e.RequestId, requestId);
        return await _dbContext.RequestResults.Find(filterDefinition).SingleAsync();
    }

    public async Task AddOrUpdateRequestInfo(CrackHashRequestResultEntity entity)
    {
        var filterDefinition = Builders<CrackHashRequestResultEntity>.Filter.Eq(e => e.RequestId, entity.RequestId);
        var updateDefinition = Builders<CrackHashRequestResultEntity>.Update
            .Set(e => e.Status, entity.Status)
            .Set(e => e.Data, entity.Data);
        var options = new UpdateOptions
        {
            IsUpsert = true
        };
        
        await _dbContext.RequestResults.UpdateOneAsync(filterDefinition, updateDefinition, options);
    }

    public async Task UpdateRequestProcessingStatus(string requestId, RequestProcessingStatus status)
    {
        var filterDefinition = Builders<CrackHashRequestResultEntity>.Filter.Eq(e => e.RequestId, requestId);
        var updateDefinition = Builders<CrackHashRequestResultEntity>.Update.Set(e => e.Status, status);
        var options = new UpdateOptions
        {
            IsUpsert = true
        };
        
        await _dbContext.RequestResults.UpdateOneAsync(filterDefinition, updateDefinition, options);
    }

    public async Task UpdateRequestData(string requestId, List<string> data)
    {
        var requestResultInfo = await GetRequestResultByRequestId(requestId);
        
        if (requestResultInfo != null)
        {
            requestResultInfo.Data.AddRange(data);
            var filterDefinition = Builders<CrackHashRequestResultEntity>.Filter.Eq(e => e.RequestId, requestId);
            var updateDefinition = Builders<CrackHashRequestResultEntity>.Update.Set<List<string>>(e => e.Data, requestResultInfo.Data);
            var options = new UpdateOptions
            {
                IsUpsert = true
            };
            
            await _dbContext.RequestResults.UpdateOneAsync(filterDefinition, updateDefinition, options);
        }
    }

    public async Task AddOrUpdateWorkerRequestProcessingStatus(CrackHashWorkerRequestProcessingStatusEntity entity)
    {
        var filterDefinition = Builders<CrackHashWorkerRequestProcessingStatusEntity>.Filter.And(
            new List<FilterDefinition<CrackHashWorkerRequestProcessingStatusEntity>>
            {
                Builders<CrackHashWorkerRequestProcessingStatusEntity>.Filter.Eq<string>(e => e.RequestId, entity.RequestId),
                Builders<CrackHashWorkerRequestProcessingStatusEntity>.Filter.Eq<int>(e => e.WorkerId, entity.WorkerId)
            });
        var updateDefinition = Builders<CrackHashWorkerRequestProcessingStatusEntity>.Update.Set(e => e.Status, entity.Status);
        var options = new UpdateOptions
        {
            IsUpsert = true
        };

        await _dbContext.WorkerRequestProcessingStatuses.UpdateOneAsync(filterDefinition, updateDefinition, options);
    }

    public async Task<List<CrackHashWorkerRequestProcessingStatusEntity>> GetWorkerRequestProcessingStatusesByRequestId(string requestId)
    {
        var filterDefinition = Builders<CrackHashWorkerRequestProcessingStatusEntity>.Filter.Eq(e => e.RequestId, requestId);
        var result = await _dbContext.WorkerRequestProcessingStatuses.FindAsync(filterDefinition);
        return result.ToList();
    }
}