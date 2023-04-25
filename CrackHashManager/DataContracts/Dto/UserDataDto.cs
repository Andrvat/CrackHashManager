namespace DataContracts.Dto;

public record UserDataDto
{
    public string Hash { get; init; }
    public int MaxLength { get; init; }
}