using DataContracts.Enum;

namespace DataContracts.Dto;

public record CrackResultDto
{
    public RequestProcessingStatus Status { get; init; } 
    public List<string>? Data { get; set; }
};