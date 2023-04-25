using DataContracts.Enum;

namespace DataContracts.Entities;

public record CrackResultEntity
{
    public RequestProcessingStatus Status { get; init; } 
    public List<string> Data { get; set; }
};