using System;
using System.Net;
using System.Net.Sockets;

class Program
{
    static void Main()
    {
            Thread thread = new Thread(delegate()
            {
                Server server = new Server("127.0.0.1", 2126);
            });
            thread.Start();

            Console.WriteLine("Server started...");
    }
}


class Server
{
    TcpListener server = null;
    int counter = 0;

    public Server(string ip, int port)
    {
        IPAddress localAddress = IPAddress.Parse(ip);
        server = new TcpListener(localAddress, port);
        server.Start();
        StartListener();
    }

    public void StartListener()
    {
        try
        {
            while (true)
            {
                Console.WriteLine("Waiting for Incoming connections ...");

                TcpClient client = server.AcceptTcpClient();
                counter += 1;
                Console.WriteLine("Connected to authorized client: {0}", counter);
                Thread thread = new Thread(new ParameterizedThreadStart(HandleConnection));
                thread.Start(client);
            }
        }
        catch (SocketException e)
        {
            Console.WriteLine("Socket Exception: {0}", e);
            server.Stop();
        }
    }

    public void HandleConnection(Object obj)
    {

        TcpClient client = (TcpClient)obj;

        NetworkStream stream = client.GetStream();
        string data = null;
        Byte[] bytes = new byte[256];
        int i;
        try
        {
            while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
            {
                string hex = BitConverter.ToString(bytes);
                data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);
                Console.WriteLine("{1}: Received: {0}", data, Thread.CurrentThread.ManagedThreadId);
                if (data != "auth token")
                {
                    stream.Close();
                    client.Close();
                }

                string str = "Device authorization successfull";
                Byte[] reply = System.Text.Encoding.ASCII.GetBytes(str);
                stream.Write(reply, 0, reply.Length);
                Console.WriteLine("{1}: Sent: {0}", str, Thread.CurrentThread.ManagedThreadId);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("Exception: {0}", e.ToString());
            client.Close();
        }
    }
}