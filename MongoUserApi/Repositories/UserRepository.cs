using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoUserApi.Configuration;
using MongoUserApi.Models;

namespace MongoUserApi.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly IMongoCollection<User> _collection;

    public UserRepository(MongoContext context, Microsoft.Extensions.Options.IOptions<MongoUserApi.Configuration.MongoSettings> options)
    {
        string collectionName = options.Value.UsersCollection;
        _collection = context.GetCollection<User>(collectionName);
        CreateIndexesAsync().GetAwaiter().GetResult();
    }

    private async Task CreateIndexesAsync()
    {
        IndexKeysDefinition<User> keys = Builders<User>.IndexKeys.Ascending(u => u.Email);
        CreateIndexModel<User> uniqueEmail = new CreateIndexModel<User>(keys, new CreateIndexOptions { Unique = true });
        await _collection.Indexes.CreateOneAsync(uniqueEmail);
    }

    public async Task<User?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        IAsyncCursor<User> cursor = await _collection.FindAsync(u => u.Id == id, cancellationToken: cancellationToken);
        User? user = await cursor.FirstOrDefaultAsync(cancellationToken);
        return user;
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        IAsyncCursor<User> cursor = await _collection.FindAsync(u => u.Email == email, cancellationToken: cancellationToken);
        User? user = await cursor.FirstOrDefaultAsync(cancellationToken);
        return user;
    }

    public async Task<List<User>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        IAsyncCursor<User> cursor = await _collection.FindAsync(_ => true, cancellationToken: cancellationToken);
        List<User> users = await cursor.ToListAsync(cancellationToken);
        return users;
    }

    public async Task<User> CreateAsync(User user, CancellationToken cancellationToken = default)
    {
        await _collection.InsertOneAsync(user, cancellationToken: cancellationToken);
        return user;
    }

    public async Task<bool> UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        ReplaceOneResult result = await _collection.ReplaceOneAsync(u => u.Id == user.Id, user, cancellationToken: cancellationToken);
        return result.IsAcknowledged && result.ModifiedCount > 0;
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        DeleteResult result = await _collection.DeleteOneAsync(u => u.Id == id, cancellationToken);
        return result.IsAcknowledged && result.DeletedCount > 0;
    }
}
