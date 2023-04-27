using DataContracts.Enum;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Manager.DataContracts.Entities;

public class CrackHashWorkerRequestProcessingStatusEntity
{
    [BsonId] 
    public ObjectId _id { get; set; }
    public string RequestId { get; set; }
    public int WorkerId { get; set; }
    public RequestProcessingStatus Status { get; init; } 
}