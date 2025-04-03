using System;
using System.Threading.Tasks;

namespace Asynchronous;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine(await GetDataAsync());
    }

    public static string GetDataFromApi()
    {
        return "Hello, World!";
    }
    
    public static async Task<string> GetDataAsync()
    {
        Console.WriteLine("Data from Asynchronous API: ");
        await Task.Delay(5000);
        return await Task.Run(() => $"{GetDataFromApi()}");
    }
}
