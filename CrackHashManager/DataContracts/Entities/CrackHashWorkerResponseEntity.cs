namespace DataContracts.Entities;

/// <summary>
/// Ответ, содержащий строки с совпадающим хэшом
/// </summary>
public class CrackHashWorkerResponseEntity
{
    /// <summary>
    /// GUID запроса
    /// </summary>
    public string RequestId { get; init; }
    
    /// <summary>
    /// Номер запроса
    /// </summary>
    public int PartNumber { get; init; }
    
    /// <summary>
    /// Строки
    /// </summary>
    public string [] Words { get; init; }
}