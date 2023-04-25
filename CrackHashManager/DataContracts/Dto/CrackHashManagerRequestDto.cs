namespace DataContracts.Dto;

/// <summary>
/// Запрос на взлом хэша в заданном пространстве строк
/// </summary>
public record CrackHashManagerRequestDto
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
    /// Общее количество частей
    /// </summary>
    public int PartCount { get; init; }
    
    /// <summary>
    /// Хэш
    /// </summary>
    public string Hash { get; init; }
    
    /// <summary>
    /// Максимальная длина последовательности
    /// </summary>
    public int MaxLength { get; init; }
    
    /// <summary>
    /// Алфавит для генерации строк
    /// </summary>
    public string? Alphabet { get; init; }
}