using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace MongoUserApi.Configuration;

public sealed class MongoContext
{
    private readonly IMongoDatabase _database;
    private readonly MongoSettings _settings;

    public MongoContext(IOptions<MongoSettings> options)
    {
        _settings = options.Value;
        MongoClient client = new MongoClient(_settings.ConnectionString);
        _database = client.GetDatabase(_settings.Database);
    }

    public IMongoCollection<TDocument> GetCollection<TDocument>(string collectionName)
    {
        IMongoCollection<TDocument> collection = _database.GetCollection<TDocument>(collectionName);
        return collection;
    }

    public IMongoCollection<TDocument> GetCollection<TDocument>()
    {
        string name = typeof(TDocument).Name.ToLowerInvariant();
        IMongoCollection<TDocument> collection = _database.GetCollection<TDocument>(name);
        return collection;
    }
}
