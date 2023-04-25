namespace DataContracts.Dto;

public record RequestInfoDto
{
    public string RequestId { get; set; } = "";

    public void SetRandomGuid()
    {
        RequestId = Guid.NewGuid().ToString();
    }
}