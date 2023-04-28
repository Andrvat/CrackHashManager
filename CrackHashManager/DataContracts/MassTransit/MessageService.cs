using System.Collections.Concurrent;

namespace DataContracts.MassTransit;

public class MessageService<T>
{
    private readonly BlockingCollection<T> _messages = new ();

    public void AddMessage(T message)
    {
        _messages.Add(message);
        Console.WriteLine($"Message service add message {message}. Messages count after: {_messages.Count}");
    }
    
    public T GetMessage()
    { 
        Console.WriteLine($"Message service handle get message. Messages count before: {_messages.Count}");
        var message = _messages.Take(); 
        return message;
    }

    public bool ContainsUnreadMessages()
    {
        return _messages.Count > 0;
    }
}