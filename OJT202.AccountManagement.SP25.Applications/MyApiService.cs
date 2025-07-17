using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

public class MyApiService
{
    private readonly HttpClient _httpClient;

    public MyApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> GetDataAsync()
    {
        var response = await _httpClient.GetAsync("https://jsonplaceholder.typicode.com/todos/1");

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadAsStringAsync();
            return result;
        }

        return "Error fetching data";
    }
}
