using DataContracts.Enum;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Manager.DataContracts.Entities;

public record CrackHashRequestResultEntity
{
    [BsonId]
    public string? RequestId { get; set; }
    public RequestProcessingStatus Status { get; set; } 
    public List<string> Data { get; set; }
};