using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MySimpleSdk.Client
{
    public class SdkClient
    {
        private readonly HttpClient _httpClient;

        public SdkClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> GetDataAsync(string endpoint)
        {
            var response = await _httpClient.GetAsync(endpoint);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> PostDataAsync(string endpoint, string jsonData)
        {
            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(endpoint, content);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
    }
}