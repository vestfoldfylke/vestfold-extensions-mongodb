using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Vestfold.Extensions.MongoDb.Services;

public class MongoDbService : IMongoDbService
{
    private readonly ILogger<MongoDbService> _logger;
    private readonly MongoClient _client;
    private readonly IMemoryCache _collectionCache;

    private readonly int _cacheExpirationMinutes;
    
    private const int DefaultCacheExpirationMinutes = 60;

    public MongoDbService(IConfiguration configuration, ILogger<MongoDbService> logger)
    {
        var connectionString = configuration["MONGODB_CONNECTION_STRING"] ??
                               throw new InvalidOperationException(
                                   "Missing MONGODB_CONNECTION_STRING in configuration");
        
        _cacheExpirationMinutes = int.TryParse(configuration["MONGODB_CACHE_EXPIRATION_MINUTES"], out var minutes)
            ? minutes
            : DefaultCacheExpirationMinutes;

        _logger = logger;
        
        _client = new MongoClient(connectionString);
        _collectionCache = new MemoryCache(new MemoryCacheOptions());
    }

    public async Task<IMongoCollection<T>> GetMongoCollection<T>(string databaseName, string collectionName, MongoDatabaseSettings? databaseSettings = null,
        MongoCollectionSettings? collectionSettings = null)
    {
        var collectionCacheKey = $"{databaseName}.{collectionName}";
        
        var collection = await _collectionCache.GetOrCreateAsync(collectionCacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_cacheExpirationMinutes);

            var database = await GetDatabase(databaseName, databaseSettings);

            var collection = await GetCollectionAsync<T>(database, collectionName, collectionSettings);

            _logger.LogInformation("Got collection '{CollectionName}' from database '{DatabaseName}'",
                collectionName, databaseName);
            
            return collection;
        });
        
        if (collection == null)
        {
            throw new InvalidOperationException($"Collection '{collectionName}' in database '{databaseName}' not found");
        }
        
        return collection;
    }

    private async Task<IMongoDatabase> GetDatabase(string databaseName, MongoDatabaseSettings? databaseSettings = null)
    {
        var databaseNames = (await _client.ListDatabaseNamesAsync()).ToList();
        if (!databaseNames.Contains(databaseName))
        {
            throw new InvalidOperationException($"Database '{databaseName}' not found");
        }
        
        return _client.GetDatabase(databaseName, databaseSettings);
    }
    
    private static async Task<IMongoCollection<T>> GetCollectionAsync<T>(IMongoDatabase database, string collectionName,
        MongoCollectionSettings? collectionSettings = null)
    {
        var collectionNames = (await database.ListCollectionNamesAsync()).ToList();
        if (!collectionNames.Contains(collectionName))
        {
            throw new InvalidOperationException($"Collection '{collectionName}' not found");
        }
        
        return await Task.FromResult(database.GetCollection<T>(collectionName, collectionSettings));
    }
}