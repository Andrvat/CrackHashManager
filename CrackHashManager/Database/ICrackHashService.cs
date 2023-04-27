using DataContracts.Dto;
using DataContracts.Entities;
using Manager.DataContracts.Entities;

namespace Manager.Database;

public interface ICrackHashService
{ 
    Task<CrackHashRequestResultEntity> GetRequestResultByRequestId(string requestId);
    Task AddNewRequestInfo(CrackHashRequestResultEntity requestResultEntity);
}