using MySql.Data.MySqlClient;

string connectionString = "server=localhost;user=root;password=myrootpass;database=dapper-tests";

try
{
    using (MySqlConnection connection = new MySqlConnection(connectionString))
    {
        connection.Open();
        Console.WriteLine("Connected to MySQL!");

        // Call the stored procedure
        using (MySqlCommand cmd = new MySqlCommand("IncrementNumbers", connection))
        {
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            
            // Add the parameter
            cmd.Parameters.AddWithValue("incrementBy", 10);

            // Execute the procedure and read results
            using (MySqlDataReader reader = cmd.ExecuteReader())
            {
                Console.WriteLine("\nUpdated numbers:");
                Console.WriteLine("ID\tValue");
                Console.WriteLine("----------------");
                
                while (reader.Read())
                {
                    int id = reader.GetInt32("Id");
                    int value = reader.GetInt32("Value");
                    Console.WriteLine($"{id}\t{value}");
                }
            }
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}

Console.WriteLine("\nPress any key to exit...");
Console.ReadKey();
