using DataContracts.Dto;
using DataContracts.Enum;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Manager.DataContracts.Entities;

public record CrackHashWorkerTaskEntity
{
    [BsonId] 
    public ObjectId _id { get; set; }
    public string RequestId { get; set; }
    public int WorkerId { get; set; }
    public string Hash { get; set; }
    public int MaxLength { get; set; }
    public RequestProcessingStatus Status { get; set; }
    public bool IsPublished { get; set; }
}