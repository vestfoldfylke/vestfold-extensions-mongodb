![NuGet Version](https://img.shields.io/nuget/v/Vestfold.Extensions.MongoDb.svg)
![NuGet Downloads](https://img.shields.io/nuget/dt/Vestfold.Extensions.MongoDb.svg)

# Vestfold.Extensions.MongoDb

Contains builder extensions to extend a dotnet core application with MongoDB functionality.

## Setting up for an Azure Function / Azure Web App

Add the following to your `local.settings.json` file:
```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    "MONGODB_CONNECTION_STRING": "mongodb+srv://<db_username>:<db_password>@<db_server>/?retryWrites=true&w=majority&appName=<app_name>",
    "MONGODB_CACHE_EXPIRATION_MINUTES": "<optional cache expiration in minutes, default is 60>"
  }
}
```

Add the following to your Program.cs file:
```csharp
var builder = FunctionsApplication.CreateBuilder(args);
builder.ConfigureFunctionsWebApplication();
builder.Services.AddVestfoldMongoDb();
builder.Build().Run();
```

## Setting up for a HostBuilder (Console app, ClassLibrary, etc.)

Add the following to your `appsettings.json` file:
```json
{
  "MONGODB_CONNECTION_STRING": "mongodb+srv://<db_username>:<db_password>@<db_server>/?retryWrites=true&w=majority&appName=<app_name>",
  "MONGODB_CACHE_EXPIRATION_MINUTES": "<optional cache expiration in minutes, default is 60>"
}
```

Add the following to your Program.cs file:
```csharp
public static async Task Main(string[] args)
{
    await Host.CreateDefaultBuilder(args)
        .ConfigureServices(services => services.AddVestfoldMongoDb())
        .Build()
        .RunAsync();
}
```

## Setting up for a WebApplicationBuilder (WebAPI, Blazor, etc.)

Add the following to your `appsettings.json` file:
```json
{
  "MONGODB_CONNECTION_STRING": "mongodb+srv://<db_username>:<db_password>@<db_server>/?retryWrites=true&w=majority&appName=<app_name>",
  "MONGODB_CACHE_EXPIRATION_MINUTES": "<optional cache expiration in minutes, default is 60>"
}
```

Add the following to your Program.cs file:
```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddVestfoldMongoDb();

var app = builder.Build();
```

## Retrieve a MongoDB collection to work with in your own code base

Inject the `IMongoDbService` into your classes.

Then you call `GetMongoCollection<T>("databaseName", "collectionName")` to retrieve a collection of type `T`.

There is also parameters for passing along a `MongoDatabaseSettings` object and a `MongoCollectionSettings` object if you need to customize the database or collection settings. But these are optional.

```csharp
public record MyEntity(string Id, string Name);

public class Something
{
    private readonly IMongoDbService _mongoDbService;
    
    public Something(IMongoDbService mongoDbService)
    {
        _mongoDbService = mongoDbService;
    }
    
    public async Task DoSomething()
    {
        // retrieve a collection of type MyEntity from the database "databaseName" and collection "collectionName"
        var collection = await _mongoDbService.GetMongoCollection<MyEntity>("databaseName", "collectionName");
        
        // specify a filter
        var filter = Builders<MyEntity>.Filter.Empty;
        
        // find documents in the collection that match the filter
        var documents = (await collection.FindAsync(filter)).ToList();
        
        // do something with the documents
        foreach (var document in documents)
        {
            Console.WriteLine($"Id: {document.Id}, Name: {document.Name}");
        }
    }
}
```