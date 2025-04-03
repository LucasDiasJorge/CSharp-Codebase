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
        return await Task.Run(() => $"Data from Asynchronous API: {GetDataFromApi()}");
    }
}
