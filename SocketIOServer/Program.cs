using System;
using System.Collections.Generic;
using Fleck;

class Program
{
    static void Main(string[] args)
    {
        var server = new WebSocketServer("ws://0.0.0.0:5000");

        var clients = new List<IWebSocketConnection>();

        server.Start(socket =>
        {
            socket.OnOpen = () =>
            {
                Console.WriteLine("Client connected!");
                clients.Add(socket);
            };

            socket.OnClose = () =>
            {
                Console.WriteLine("Client disconnected!");
                clients.Remove(socket);
            };

            socket.OnMessage = message =>
            {
                Console.WriteLine("Received: " + message);

                // Responder como um servidor Socket.IO
                foreach (var client in clients)
                {
                    client.Send($"\{\"message\",\"Server: {message}\"}\");
                }
            };
        });

        Console.WriteLine("Socket.IO-like WebSocket server running on ws://localhost:5000");
        Console.ReadLine();
    }
}
