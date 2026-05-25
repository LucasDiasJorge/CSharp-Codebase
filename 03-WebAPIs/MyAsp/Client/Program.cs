using System;
using System.Net.Sockets;
using System.Text;

namespace MyAspClient
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Client starting...");
                Client client = new Client("127.0.0.1", 2126);
                client.Run();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }

    class Client
    {
        private string ip;
        private int port;

        public Client(string ip, int port)
        {
            this.ip = ip;
            this.port = port;
        }

        public void Run()
        {
            Console.WriteLine($"Connecting to {ip}:{port}...");
            TcpClient tcpClient = new TcpClient();
            tcpClient.Connect(ip, port);
            Console.WriteLine("Connected.");

            NetworkStream stream = tcpClient.GetStream();

            string token = "auth token";
            byte[] sendBytes = Encoding.ASCII.GetBytes(token);
            stream.Write(sendBytes, 0, sendBytes.Length);
            Console.WriteLine($"Sent: {token}");

            // Read response from server
            byte[] buffer = new byte[1024];
            int bytesRead = stream.Read(buffer, 0, buffer.Length);
            if (bytesRead > 0)
            {
                string response = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                Console.WriteLine($"Received: {response}");
            }
            else
            {
                Console.WriteLine("No response received.");
            }

            stream.Close();
            tcpClient.Close();
            Console.WriteLine("Connection closed.");
        }
    }
}
