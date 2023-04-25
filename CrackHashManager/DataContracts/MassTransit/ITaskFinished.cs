using System.Xml.Serialization;

namespace DataContracts.MassTransit;

[XmlType(AnonymousType = true, Namespace = "http://ccfit.nsu.ru/schema/crack-hash-response"),
 XmlRoot(ElementName = "CrackHashWorkerResponse", Namespace = "http://ccfit.nsu.ru/schema/crack-hash-response")]
public interface ITaskFinished
{
    // <summary>
    /// GUID запроса
    /// </summary>
    [XmlElement("RequestId", ElementName = "RequestId")]
    public string RequestId { get; set; }
    
    /// <summary>
    /// Номер части запроса
    /// </summary>
    [XmlElement("PartNumber", ElementName = "PartNumber")]
    public int PartNumber { get; set; }
    
    /// <summary>
    /// Строки
    /// </summary>
    [XmlArray(ElementName = "Words")]
    public string [] Words { get; set; }
}