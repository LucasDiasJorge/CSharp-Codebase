using StackExchange.Redis;
using System;

class Program
{
    static void Main()
    {
        try
        {
            Console.WriteLine("Connecting to Redis...");
            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("localhost");
            Console.WriteLine("Connected successfully!");

            // Get the database instance
            IDatabase db = redis.GetDatabase();

            // Debugging: Check if connection is active
            if (!redis.IsConnected)
            {
                Console.WriteLine("Redis connection failed!");
                return;
            }

            // Set a key-value pair
            Console.WriteLine("Setting key-value pair...");
            bool isSet = db.StringSet("myKey", "Hello, Redis!");

            if (isSet)
            {
                Console.WriteLine("Value stored successfully!");
            }
            else
            {
                Console.WriteLine("Failed to store value!");
            }

            // Retrieve the value
            Console.WriteLine("Retrieving value...");
            string value = db.StringGet("myKey");

            if (!string.IsNullOrEmpty(value))
            {
                Console.WriteLine($"Stored value: {value}");
            }
            else
            {
                Console.WriteLine("Failed to retrieve value or value does not exist!");
            }

            // Close the connection
            Console.WriteLine("Closing Redis connection...");
            redis.Close();
            Console.WriteLine("Connection closed.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}