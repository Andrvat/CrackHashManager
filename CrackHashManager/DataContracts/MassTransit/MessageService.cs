using System.Collections.Concurrent;

namespace DataContracts.MassTransit;

public class MessageService<T>
{
    private readonly BlockingCollection<T> _messages = new ();

    public void AddMessage(T message)
    {
        _messages.Add(message);
    }
    
    public T GetMessage()
    { 
        var message = _messages.Take(); 
        return message;
    }
}