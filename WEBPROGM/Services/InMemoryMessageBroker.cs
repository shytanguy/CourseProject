using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;


public class InMemoryMessageBroker : IMessageBroker
{
    private readonly ConcurrentDictionary<string, ConcurrentQueue<string>> _queues = new();
    private readonly ConcurrentDictionary<string, List<Action<string>>> _subscribers = new();

    public void Publish(string queueName, string message)
    {
        
        var queue = _queues.GetOrAdd(queueName, new ConcurrentQueue<string>());
        queue.Enqueue(message);

       
        if (_subscribers.ContainsKey(queueName))
        {
            foreach (var handler in _subscribers[queueName])
            {
                Task.Run(() => handler(message));
            }
        }
    }

    public void Subscribe(string queueName, Action<string> handler)
    {
        var handlers = _subscribers.GetOrAdd(queueName, _ => new List<Action<string>>());
        handlers.Add(handler);

      
        if (_queues.TryGetValue(queueName, out var queue))
        {
            while (queue.TryDequeue(out var message))
            {
                Task.Run(() => handler(message));
            }
        }
    }
}
