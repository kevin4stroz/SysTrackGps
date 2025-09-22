using System;

namespace WorkerConsumer.Service;

public interface IMessageProcessor
{
    Task ProcessMessageAsync(string message);
}
