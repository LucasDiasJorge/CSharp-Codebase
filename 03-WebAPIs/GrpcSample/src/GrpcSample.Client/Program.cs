using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Net.Client;
using GrpcSample.Contracts.Users.V1;

namespace GrpcSample.Client
{
	internal static class Program
	{
		private static async Task<int> Main(string[] args)
		{
			// Address must match server's configured URL. Use HTTP/2.
			string serverAddress = Environment.GetEnvironmentVariable("GRPC_SERVER_ADDRESS") ?? "https://localhost:7156";

			using GrpcChannel channel = GrpcChannel.ForAddress(serverAddress);
			UserService.UserServiceClient client = new UserService.UserServiceClient(channel);

			CancellationTokenSource cts = new CancellationTokenSource();
			CancellationToken cancellationToken = cts.Token;

			// Unary: create user
			Console.WriteLine("Creating a user via unary call...");
			CreateUserRequest createRequest = new CreateUserRequest { Name = "Alice Example", Email = "alice@example.com" };
			CreateUserResponse createResponse = await client.CreateUserAsync(createRequest, cancellationToken: cancellationToken);
			Console.WriteLine($"Created user: {createResponse.User.Id} - {createResponse.User.Name}");

			// Unary: get user
			Console.WriteLine("Fetching the created user...");
			GetUserRequest getRequest = new GetUserRequest { Id = createResponse.User.Id };
			GetUserResponse getResponse = await client.GetUserAsync(getRequest, cancellationToken: cancellationToken);
			Console.WriteLine($"Fetched user: {getResponse.User.Id} - {getResponse.User.Email}");

			// Server streaming: list users (page 1, size 10)
			Console.WriteLine("Streaming users (server streaming)...");
			ListUsersRequest listRequest = new ListUsersRequest { PageNumber = 1, PageSize = 10 };
			using AsyncServerStreamingCall<ListUsersResponse> listCall = client.ListUsers(listRequest, cancellationToken: cancellationToken);
			while (await listCall.ResponseStream.MoveNext(cancellationToken).ConfigureAwait(false))
			{
				ListUsersResponse item = listCall.ResponseStream.Current;
				Console.WriteLine($"User: {item.User.Id} - {item.User.Name}");
			}

			// Client streaming: import users
			Console.WriteLine("Importing users (client streaming)...");
			using AsyncClientStreamingCall<ImportUsersRequest, ImportUsersResponse> importCall = client.ImportUsers(cancellationToken: cancellationToken);
			List<User> toImport = new List<User>
			{
				new User { Name = "Bob Builder", Email = "bob@example.com" },
				new User { Name = "Carol Coder", Email = "carol@example.com" }
			};
			foreach (User user in toImport)
			{
				ImportUsersRequest importRequest = new ImportUsersRequest { User = user };
				await importCall.RequestStream.WriteAsync(importRequest).ConfigureAwait(false);
			}
			await importCall.RequestStream.CompleteAsync().ConfigureAwait(false);
			ImportUsersResponse importResponse = await importCall.ResponseAsync.ConfigureAwait(false);
			Console.WriteLine($"Imported users: {importResponse.ImportedCount}");

			// Bidirectional streaming: echo a few events
			Console.WriteLine("Bi-di streaming user events...");
			using AsyncDuplexStreamingCall<UserEvent, UserEvent> eventsCall = client.UserEvents(cancellationToken: cancellationToken);

			Task reader = Task.Run(async () =>
			{
				while (await eventsCall.ResponseStream.MoveNext(cancellationToken).ConfigureAwait(false))
				{
					UserEvent serverEvent = eventsCall.ResponseStream.Current;
					Console.WriteLine($"[Server] {serverEvent.Message} (user: {serverEvent.UserId})");
				}
			}, cancellationToken);

			UserEvent e1 = new UserEvent { Message = "Client event 1", UserId = createResponse.User.Id };
			UserEvent e2 = new UserEvent { Message = "Client event 2", UserId = createResponse.User.Id };
			await eventsCall.RequestStream.WriteAsync(e1).ConfigureAwait(false);
			await eventsCall.RequestStream.WriteAsync(e2).ConfigureAwait(false);
			await eventsCall.RequestStream.CompleteAsync().ConfigureAwait(false);
			await reader.ConfigureAwait(false);

			Console.WriteLine("Done.");
			return 0;
		}
	}
}
