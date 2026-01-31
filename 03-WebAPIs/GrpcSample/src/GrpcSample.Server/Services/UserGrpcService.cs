using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Grpc.Core;
using GrpcSample.Contracts.Users.V1;

namespace GrpcSample.Server.Services
{
    // Repository abstraction to keep code testable and clean
    public interface IUserRepository
    {
        Task<User> CreateAsync(string name, string email, CancellationToken cancellationToken);
        Task<User?> GetAsync(string id, CancellationToken cancellationToken);
        IAsyncEnumerable<User> ListAsync(int pageNumber, int pageSize, CancellationToken cancellationToken);
        Task<int> ImportAsync(IAsyncEnumerable<User> users, CancellationToken cancellationToken);
    }

    // In-memory repository implementation, suitable for demo and tests
    public sealed class InMemoryUserRepository : IUserRepository
    {
        private readonly ConcurrentDictionary<string, User> storage = new ConcurrentDictionary<string, User>();

        public Task<User> CreateAsync(string name, string email, CancellationToken cancellationToken)
        {
            string id = Guid.NewGuid().ToString("N");
            User created = new User { Id = id, Name = name, Email = email };
            storage[id] = created;
            return Task.FromResult(created);
        }

        public Task<User?> GetAsync(string id, CancellationToken cancellationToken)
        {
            bool found = storage.TryGetValue(id, out User? user);
            return Task.FromResult(found ? user : null);
        }

        public async IAsyncEnumerable<User> ListAsync(int pageNumber, int pageSize, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            if (pageNumber < 1)
            {
                pageNumber = 1;
            }
            if (pageSize < 1)
            {
                pageSize = 10;
            }

            IEnumerable<User> all = storage.Values
                .OrderBy(u => u.Name, StringComparer.OrdinalIgnoreCase)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize);

            foreach (User user in all)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await Task.Yield();
                yield return user;
            }
        }

        public async Task<int> ImportAsync(IAsyncEnumerable<User> users, CancellationToken cancellationToken)
        {
            int count = 0;
            await foreach (User user in users.WithCancellation(cancellationToken))
            {
                if (string.IsNullOrWhiteSpace(user.Id))
                {
                    user.Id = Guid.NewGuid().ToString("N");
                }
                storage[user.Id] = new User { Id = user.Id, Name = user.Name, Email = user.Email };
                count++;
            }
            return count;
        }
    }

    // gRPC service implementation: covers unary, server-streaming, client-streaming and bidi-streaming
    public sealed class UserGrpcService : UserService.UserServiceBase
    {
        private readonly ILogger<UserGrpcService> logger;
        private readonly IUserRepository repository;

        public UserGrpcService(ILogger<UserGrpcService> logger, IUserRepository repository)
        {
            this.logger = logger;
            this.repository = repository;
        }

        // Unary: fetch single user by id
        public override async Task<GetUserResponse> GetUser(GetUserRequest request, ServerCallContext context)
        {
            CancellationToken cancellationToken = context.CancellationToken;
            User? user = await repository.GetAsync(request.Id, cancellationToken).ConfigureAwait(false);
            if (user == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, $"User {request.Id} not found"));
            }

            GetUserResponse response = new GetUserResponse { User = user };
            return response;
        }

        // Unary: create new user
        public override async Task<CreateUserResponse> CreateUser(CreateUserRequest request, ServerCallContext context)
        {
            CancellationToken cancellationToken = context.CancellationToken;
            if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Email))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Name and Email are required"));
            }

            User created = await repository.CreateAsync(request.Name, request.Email, cancellationToken).ConfigureAwait(false);
            CreateUserResponse response = new CreateUserResponse { User = created };
            return response;
        }

        // Server streaming: stream users for a page
        public override async Task ListUsers(ListUsersRequest request, IServerStreamWriter<ListUsersResponse> responseStream, ServerCallContext context)
        {
            CancellationToken cancellationToken = context.CancellationToken;
            await foreach (User user in repository.ListAsync(request.PageNumber, request.PageSize, cancellationToken))
            {
                ListUsersResponse response = new ListUsersResponse { User = user };
                await responseStream.WriteAsync(response).ConfigureAwait(false);
            }
        }

        // Client streaming: import users
        public override async Task<ImportUsersResponse> ImportUsers(IAsyncStreamReader<ImportUsersRequest> requestStream, ServerCallContext context)
        {
            CancellationToken cancellationToken = context.CancellationToken;
            async IAsyncEnumerable<User> ReadUsers()
            {
                while (await requestStream.MoveNext(cancellationToken).ConfigureAwait(false))
                {
                    ImportUsersRequest current = requestStream.Current;
                    yield return current.User;
                }
            }

            int imported = await repository.ImportAsync(ReadUsers(), cancellationToken).ConfigureAwait(false);
            ImportUsersResponse response = new ImportUsersResponse { ImportedCount = imported };
            return response;
        }

        // Bidirectional streaming: echo user events back with server annotation
        public override async Task UserEvents(IAsyncStreamReader<UserEvent> requestStream, IServerStreamWriter<UserEvent> responseStream, ServerCallContext context)
        {
            CancellationToken cancellationToken = context.CancellationToken;
            while (await requestStream.MoveNext(cancellationToken).ConfigureAwait(false))
            {
                UserEvent incoming = requestStream.Current;
                UserEvent outgoing = new UserEvent
                {
                    Message = $"Server received: {incoming.Message}",
                    UserId = incoming.UserId
                };
                await responseStream.WriteAsync(outgoing).ConfigureAwait(false);
            }
        }
    }
}
