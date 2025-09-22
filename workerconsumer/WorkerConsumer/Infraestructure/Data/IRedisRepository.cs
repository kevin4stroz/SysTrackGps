using System;
using WorkerConsumer.Infraestructure.Entities;

namespace WorkerConsumer.Infraestructure.Data;

public interface IRedisRepository
{
    Task AddToListAsync<T>(string key, T newItem, TimeSpan? expiry = null);
    Task<LocalidadRedis?> GetLocalidadByNameAsync(string name);
}
