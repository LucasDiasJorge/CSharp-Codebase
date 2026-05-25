using System.Net.Sockets;

namespace Server;

class Program
{
    static void Main(string[] args)
    {
        Server server = new Server();
        try
        {
            server.Start();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}

class Server
{

    private Worker worker = new Worker();
    public void Start()
    {
        Console.WriteLine("Server started...");
        TcpListener server = new TcpListener(System.Net.IPAddress.Any, 2621);
        server.Start();

        Socket client = server.AcceptSocket();
        Console.WriteLine("Client connected to server.");
        
        worker.Job(client);

        client.Close();
        server.Stop();
    }
}

class Worker
{
    Random random = new Random();

    public void Job(Socket client)
    {
        Console.WriteLine("Worker is processing client request...");
        SendMensage(client, "Hello from the server!");
        Console.WriteLine("Worker has finished processing client request.");
    }

    public void SendMensage(Socket client, string message)
    {
        client.Send(System.Text.Encoding.UTF8.GetBytes(message));
    }
}