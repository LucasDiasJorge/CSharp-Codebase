using System;
using System.Collections.Generic;
using SocketIO.Server;
using SocketIOSharp.Common.Abstract.Connection;
using SocketIOSharp.Server;

class Program
{
    static void Main(string[] args)
    {
        // Criação do servidor Socket.IO
        var server = new SocketIOServer("http://0.0.0.0:5000");

        var clients = new Dictionary<string, SocketIOConnection<>>();

        server.OnConnection((socket) =>
        {
            var path = socket.Request.Path;

            // Verifica se o path contém um "mac" (exemplo: /{mac})
            if (!string.IsNullOrEmpty(path) && path.StartsWith("/"))
            {
                var macAddress = path.Substring(1);  // Remove a barra "/"
                Console.WriteLine($"Client with MAC {macAddress} connected!");
                clients[macAddress] = socket;  // Armazenar a conexão
            }
            else
            {
                Console.WriteLine("Client connected with an invalid path.");
                socket.Close();  // Fecha a conexão caso o path não seja válido
            }

            // Evento de desconexão
            socket.OnDisconnect(() =>
            {
                if (!string.IsNullOrEmpty(path) && path.StartsWith("/"))
                {
                    var macAddress = path.Substring(1);
                    Console.WriteLine($"Client with MAC {macAddress} disconnected!");
                    clients.Remove(macAddress);  // Remove o cliente da lista
                }
            });

            // Evento de recebimento de mensagens
            socket.On("message", (message, ack) =>
            {
                Console.WriteLine("Received: " + message);

                // Enviar a mensagem para todos os clientes conectados
                foreach (var client in clients.Values)
                {
                    var data = new Dictionary<string, string>
                    {
                        { "message", message.ToString() }
                    };

                    client.Emit("message", data);  // Enviar a mensagem com a chave "message"
                }

                // Enviar um ACK (acknowledgment) para o cliente após o envio
                ack("Message delivered successfully!");
            });
        });

        Console.WriteLine("Socket.IO server running on http://localhost:5000");
        Console.ReadLine();
    }
}
