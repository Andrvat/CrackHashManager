using DataContracts.Dto;
using DataContracts.Enum;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Manager.DataContracts.Entities;

public class CrackHashWorkerTaskEntity
{
    [BsonId] 
    public ObjectId _id { get; set; }
    public string RequestId { get; set; }
    public int WorkerId { get; set; }
    public string Hash { get; init; }
    public int MaxLength { get; init; }
    public RequestProcessingStatus Status { get; set; } 
}