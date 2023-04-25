namespace DataContracts.Dto;

/// <summary>
/// Ответ, содержащий строки с совпадающим хэшом
/// </summary>
public record CrackHashWorkerResponseDto
{
    /// <summary>
    /// GUID запроса
    /// </summary>
    public string RequestId { get; init; }
    
    /// <summary>
    /// Номер части запроса
    /// </summary>
    public int PartNumber { get; init; }
    
    /// <summary>
    /// Строки
    /// </summary>
    public string [] Words { get; init; }
}