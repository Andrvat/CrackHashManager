namespace DataContracts.MassTransit;

public class MessageService<T>
{
    private T? _message;
    private TaskCompletionSource<T>? _tcs;

    public void NextMessage(T message)
    {
        _message = message;
        _tcs?.SetResult(message);
        _tcs = null;
    }

    public async Task<T> AwaitMessage()
    {
        if (_message != null)
        {
            return _message;
        }

        _tcs = new TaskCompletionSource<T>();
        return await _tcs.Task;
    }
}