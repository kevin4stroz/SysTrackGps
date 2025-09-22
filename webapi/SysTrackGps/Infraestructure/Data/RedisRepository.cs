using SysTrackGps.Domain.Entities.Redis;
using NRedisGraph;
using StackExchange.Redis;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using SysTrackGps.Utilities;
using SysTrackGps.Domain.Entities.AlgoritmoA;


namespace SysTrackGps.Infraestructure.Data;

public class RedisRepository : IRedisRepository
{
    private readonly IConnectionMultiplexer _redisConnection;
    private readonly IDistributedCache _cache;
    private readonly IDatabase _redisDatabase;
    private const string GRAPH_NAME = "CityGraph";
    private const string CACHE_KEY_LOCALIDADES = "localidades_all";

    public RedisRepository(IConnectionMultiplexer redisConnection, IDistributedCache cache)
    {
        _redisConnection = redisConnection;
        _cache = cache;
        _redisDatabase = _redisConnection.GetDatabase();
    }

    public async Task<IEnumerable<LocalidadRedis>> GetAllLocalidadesAsync()
    {
        // intentar obtener el cache primero
        string? cacheData = await _cache.GetStringAsync(CACHE_KEY_LOCALIDADES);

        if (!string.IsNullOrEmpty(cacheData))
        {
            return JsonSerializer.Deserialize<List<LocalidadRedis>>(cacheData)!;
        }

        // como no encontro cache, lo consulta y lo crea
        RedisGraph graph = new RedisGraph(_redisConnection.GetDatabase());

        string query = @"
            MATCH (l:Localidad)
            RETURN DISTINCT l.name AS name, l.lat AS lat, l.lon AS lon
            ORDER BY name";

        ResultSet result = await graph.QueryAsync(GRAPH_NAME, query);

        List<LocalidadRedis> localidades = new List<LocalidadRedis>();

        foreach (Record record in result)
        {
            LocalidadRedis localidad = new LocalidadRedis()
            {
                Name = record.GetValue<string>("name"),
                Lat = record.GetValue<double>("lat"),
                Lon = record.GetValue<double>("lon")
            };

            localidades.Add(localidad);
        }

        // crear localidad por 5 minutos
        DistributedCacheEntryOptions cacheOptions = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
        };

        await _cache.SetStringAsync(
            CACHE_KEY_LOCALIDADES,
            JsonSerializer.Serialize(localidades),
            cacheOptions
        );

        return localidades;
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

    public async Task<AStarResult> FindShortestPathAsync(string start, string end)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        var startNode = await GetLocalidadByNameAsync(start);
        var endNode = await GetLocalidadByNameAsync(end);

        if (startNode == null || endNode == null)
            return new AStarResult { Path = new List<LocalidadRedis>() };

        var openSet = new PriorityQueue<AStarNode, double>();
        var closedSet = new Dictionary<string, AStarNode>();
        var nodesVisited = 0;

        // Nodo inicial
        var startAStarNode = new AStarNode
        {
            Name = startNode.Name,
            Lat = startNode.Lat,
            Lon = startNode.Lon,
            GCost = 0,
            HCost = GeoCalculator.CalculateDistance(startNode.Lat, startNode.Lon, endNode.Lat, endNode.Lon)
        };

        openSet.Enqueue(startAStarNode, startAStarNode.FCost);

        while (openSet.Count > 0)
        {
            nodesVisited++;
            var currentNode = openSet.Dequeue();

            // ¿Llegamos al objetivo?
            if (currentNode.Name == end)
            {
                stopwatch.Stop();
                return new AStarResult
                {
                    Path = ReconstructPath(currentNode),
                    TotalDistance = currentNode.GCost,
                    NodesVisited = nodesVisited,
                    ExecutionTime = stopwatch.Elapsed
                };
            }

            closedSet[currentNode.Name] = currentNode;

            // Obtener vecinos
            var neighbors = await GetNeighborsAsync(currentNode.Name);

            foreach (var neighborName in neighbors)
            {
                if (closedSet.ContainsKey(neighborName))
                    continue;

                var neighborData = await GetLocalidadByNameAsync(neighborName);
                if (neighborData == null)
                    continue;

                // Calcular costos
                var distanceToNeighbor = GeoCalculator.CalculateDistance(
                    currentNode.Lat, currentNode.Lon,
                    neighborData.Lat, neighborData.Lon);

                var tentativeGCost = currentNode.GCost + distanceToNeighbor;

                var neighborNode = new AStarNode
                {
                    Name = neighborData.Name,
                    Lat = neighborData.Lat,
                    Lon = neighborData.Lon,
                    GCost = tentativeGCost,
                    HCost = GeoCalculator.CalculateDistance(
                        neighborData.Lat, neighborData.Lon,
                        endNode.Lat, endNode.Lon),
                    Parent = currentNode
                };

                // Verificar si ya está en openSet con mejor costo
                var existingNode = openSet.UnorderedItems
                    .FirstOrDefault(x => x.Element.Name == neighborName).Element;

                if (existingNode == null || tentativeGCost < existingNode.GCost)
                {
                    openSet.Enqueue(neighborNode, neighborNode.FCost);
                }
            }
        }

        stopwatch.Stop();
        return new AStarResult { Path = new List<LocalidadRedis>(), NodesVisited = nodesVisited };
    }

    private List<LocalidadRedis> ReconstructPath(AStarNode endNode)
    {
        var path = new List<LocalidadRedis>();
        var currentNode = endNode;

        while (currentNode != null)
        {
            path.Insert(0, new LocalidadRedis
            {
                Name = currentNode.Name,
                Lat = currentNode.Lat,
                Lon = currentNode.Lon
            });
            currentNode = currentNode.Parent;
        }

        return path;
    }

    public async Task<List<string>> GetNeighborsAsync(string localidadName)
    {
        var graph = new RedisGraph(_redisConnection.GetDatabase());

        var query = @"
            MATCH (l:Localidad {name: $name})-[:CONNECTED]-(vecino:Localidad)
            RETURN vecino.name as name";

        var parameters = new Dictionary<string, object> { { "name", localidadName } };
        var result = await graph.QueryAsync(GRAPH_NAME, query, parameters);

        var neighbors = new List<string>();

        foreach (Record record in result)
        {
            neighbors.Add(record.GetValue<string>("name"));
        }

        return neighbors;
    }
    

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
    {
        var serializedValue = JsonSerializer.Serialize(value);
        await _redisDatabase.StringSetAsync(key, serializedValue, expiry);
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        var value = await _redisDatabase.StringGetAsync(key);
        
        if (value.IsNullOrEmpty)
            return default;
            
        return JsonSerializer.Deserialize<T>(value!);
    }

    public async Task<bool> RemoveAsync(string key)
    {
        return await _redisDatabase.KeyDeleteAsync(key);
    }


}
