namespace DataContracts.Enum;

public enum RequestProcessingStatus
{
    /// <summary>
    /// В обработке
    /// </summary>
    InProgress = 0,
    
    /// <summary>
    /// Выполнен
    /// </summary>
    Ready = 1,
    
    /// <summary>
    /// Ошибка
    /// </summary>
    Error = 2
}