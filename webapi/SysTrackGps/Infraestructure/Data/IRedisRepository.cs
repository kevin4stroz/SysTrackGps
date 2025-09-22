using System;
using SysTrackGps.Domain.Entities.AlgoritmoA;
using SysTrackGps.Domain.Entities.Redis;

namespace SysTrackGps.Infraestructure.Data;

public interface IRedisRepository
{
    // Metodos especificos del grafo
    Task<IEnumerable<LocalidadRedis>> GetAllLocalidadesAsync();
    Task<LocalidadRedis?> GetLocalidadByNameAsync(string name);
    Task<AStarResult> FindShortestPathAsync(string start, string end);
    Task<List<string>> GetNeighborsAsync(string localidadName);

    // metodos genericos
    Task SetAsync<T>(string key, T value, TimeSpan? expiry = null);
    Task<T?> GetAsync<T>(string key);
    Task<bool> RemoveAsync(string key);
}
