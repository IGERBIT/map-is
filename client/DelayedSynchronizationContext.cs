using System.Collections.Concurrent;

namespace client;

public class DelayedSynchronizationContext : SynchronizationContext
{
    private readonly ConcurrentQueue<(SendOrPostCallback, object?)> _messages = new ();
    private readonly object _lockObject = new();
    

    public override void Post(SendOrPostCallback d, object? state)
    {
        _messages.Enqueue((d,state));
    }

    public override void Send(SendOrPostCallback d, object? state)
    {
        throw new NotSupportedException("Send not supported by this context");
    }
    
    public override SynchronizationContext CreateCopy()
    {
        return this;
    }


    public void ProcessMessages()
    {
        if(_messages.IsEmpty) return;

        while (_messages.TryDequeue(out var tuple))
        {
            tuple.Item1(tuple.Item2);
        }
    }
}

