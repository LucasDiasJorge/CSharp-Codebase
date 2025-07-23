// filepath: c:\Users\Lucas Jorge\Documents\Default Projects\Back\CSharp-101\MySimpleSdk\MySimpleSdk.Demo\Program.cs
using System;
using MySimpleSdk;
using MySimpleSdk.Models;
using MySimpleSdk.Services;

namespace MySimpleSdk.Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            // Create an instance of the SdkClient
            SdkClient client = new SdkClient();

            // Fetch data using the SdkService
            SdkService service = new SdkService(client);
            SdkModel data = service.FetchData();

            // Display the fetched data
            Console.WriteLine($"Id: {data.Id}, Name: {data.Name}, Description: {data.Description}");

            // Example of posting data
            SdkModel newData = new SdkModel
            {
                Id = 2,
                Name = "New Item",
                Description = "This is a new item."
            };

            service.SaveData(newData);
            Console.WriteLine("Data saved successfully.");
        }
    }
}