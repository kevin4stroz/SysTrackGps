using System;
using System.Text.Json;
using NRedisGraph;
using StackExchange.Redis;
using WorkerConsumer.Infraestructure.Entities;

namespace WorkerConsumer.Infraestructure.Data;

public class RedisRepository : IRedisRepository
{

    private readonly IConnectionMultiplexer _redisConnection;
    private readonly IDatabase _redisDatabase;
    private const string GRAPH_NAME = "CityGraph";

    public RedisRepository(IConnectionMultiplexer redisConnection)
    {
        _redisConnection = redisConnection;
        _redisDatabase = _redisConnection.GetDatabase();
    }

    public async Task AddToListAsync<T>(string key, T newItem, TimeSpan? expiry = null)
    {
        var exists = await _redisDatabase.KeyExistsAsync(key);

        List<T> list;

        if (exists)
        {
            var serializedList = await _redisDatabase.StringGetAsync(key);
            list = JsonSerializer.Deserialize<List<T>>(serializedList!) ?? new List<T>();
        }
        else
        {
            list = new List<T>();
        }

        list.Add(newItem);

        var updatedSerializedList = JsonSerializer.Serialize(list);
        await _redisDatabase.StringSetAsync(key, updatedSerializedList, expiry);
    }

    public async Task<LocalidadRedis?> GetLocalidadByNameAsync(string name)
    {
        var graph = new RedisGraph(_redisConnection.GetDatabase());

        var query = @"
            MATCH (l:Localidad {name: $name}) 
            RETURN l.name as name, l.lat as lat, l.lon as lon";

        var parameters = new Dictionary<string, object> { { "name", name } };
        var result = await graph.QueryAsync(GRAPH_NAME, query, parameters);

        if (result.Count == 0)
            return null;

        var record = result.First();
        return new LocalidadRedis
        {
            Name = record.GetValue<string>("name"),
            Lat = record.GetValue<double>("lat"),
            Lon = record.GetValue<double>("lon")
        };
    }

}
