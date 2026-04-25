using System;
using System.Data;
using MySql.Data.MySqlClient;
using StackExchange.Redis;

class Program
{
    static void Main()
    {
        string mysqlConnStr = "server=localhost;user=root;password=myrootpass;database=my-db";
        string redisConnStr = "localhost";

        try
        {
            // Connect to MySQL
            using MySqlConnection mysqlConn = new MySqlConnection(mysqlConnStr);
            mysqlConn.Open();
            Console.WriteLine("Connected to MySQL!");

            // Query data from MySQL
            string query = "SELECT id, nome FROM usuarios"; // Modify based on your table
            using MySqlCommand cmd = new MySqlCommand(query, mysqlConn);
            using MySqlDataReader reader = cmd.ExecuteReader();

            // Connect to Redis
            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(redisConnStr);
            IDatabase db = redis.GetDatabase();
            Console.WriteLine("Connected to Redis!");

            while (reader.Read())
            {
                string key = $"usuario:{reader["id"]}";
                string value = reader["nome"].ToString();

                // Insert data into Redis
                db.StringSet(key, value);
                Console.WriteLine($"Inserted into Redis: {key} -> {value}");
            }

            Console.WriteLine("Data transfer completed!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}