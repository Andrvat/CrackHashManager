using System.Net;
using DataContracts.Dto;
using DataContracts.Entities;
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
    
    public async Task<CrackHashRequestResultEntity> GetRequestResultByRequestId(string requestId)
    {
        var filter = Builders<CrackHashRequestResultEntity>.Filter.Eq(e => e.RequestId, requestId);

        return await GetDataFromDb(filter);
    }

    public async Task AddNewRequestInfo(CrackHashRequestResultEntity requestResultEntity)
    {
        await _dbContext.RequestResults.InsertOneAsync(requestResultEntity);
    }

    private async Task<CrackHashRequestResultEntity> GetDataFromDb(FilterDefinition<CrackHashRequestResultEntity> finalFilter)
    {
        var result = await _dbContext.RequestResults.Find(finalFilter).SingleAsync();
        return result;
    }
}