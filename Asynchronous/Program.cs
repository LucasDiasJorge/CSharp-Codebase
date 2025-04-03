using System;
using System.Threading.Tasks;

namespace Asynchronous;

class Program
{
    static async Task Main(string[] args)
    {

        Console.WriteLine(await GetDataAsync());
        
        Task<string> data1 = GetDataAsync();
        Task<string> data2 = GetDataAsync();
        await Task.WhenAll(data1, data2);
        Console.WriteLine(data1.Result);
        Console.WriteLine(data2.Result);
        
        await Task.Run(() => PerformRequestWithHandling());
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

    public static async Task PerformRequestWithHandling()
    {
        try
        {
            HttpClient client = new HttpClient();
            string data = await client.GetStringAsync("https://example.com/data");
            Console.WriteLine(data);
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine("\nException Caught!");
            Console.WriteLine("Message :{0} ", e.Message);
        }
    }
    
}
