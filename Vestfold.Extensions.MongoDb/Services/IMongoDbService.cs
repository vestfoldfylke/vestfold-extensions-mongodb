using MongoDB.Driver;

namespace Vestfold.Extensions.MongoDb.Services;

public interface IMongoDbService
{
    Task<IMongoCollection<T>> GetMongoCollection<T>(string databaseName, string collectionName,
        MongoDatabaseSettings? databaseSettings = null,
        MongoCollectionSettings? collectionSettings = null);
}