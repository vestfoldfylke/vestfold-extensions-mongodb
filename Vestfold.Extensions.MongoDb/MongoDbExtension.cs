using Microsoft.Extensions.DependencyInjection;
using Vestfold.Extensions.MongoDb.Services;

namespace Vestfold.Extensions.MongoDb;

public static class MongoDbExtension
{
    public static IServiceCollection AddVestfoldMongoDb(this IServiceCollection services) =>
        services.AddSingleton<IMongoDbService, MongoDbService>();
}