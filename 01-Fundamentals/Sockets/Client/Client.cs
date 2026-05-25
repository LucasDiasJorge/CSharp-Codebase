using System.Net.Sockets;

namespace Client;

class Program
{
    static void Main(string[] args)
    {
        Client client = new Client();
        try
        {
            client.Connect();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}

class Client
{
    public void Connect()
    {
        Console.WriteLine("Client connecting...");
        TcpClient client = new TcpClient();
        client.Connect("localhost", 2621);
        Console.WriteLine("Client connected to server.");
        Console.WriteLine($"Received from server: {Read(client)}");
        client.Close();
    }

    public string Read(TcpClient client)
    {
        client.GetStream().ReadTimeout = 5000; // Set a timeout for reading data
        byte[] buffer = new byte[1024];
        int bytesRead = client.GetStream().Read(buffer, 0, buffer.Length);
        string message = System.Text.Encoding.UTF8.GetString(buffer, 0, bytesRead);
        return message;
    }
}