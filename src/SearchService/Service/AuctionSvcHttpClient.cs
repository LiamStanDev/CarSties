using System.Net.Http.Json;
using MongoDB.Entities;
using SearchService.Entities;

namespace SearchService.Service;

public class AuctionSvcHttpClient {
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;

    public AuctionSvcHttpClient(HttpClient httpClient, IConfiguration config) {
        _httpClient = httpClient;
        _config = config;
    }


    public async Task<List<Item>> GetItemsForSearchDB() {
        var lastUpdated = await DB.Find<Item, string>()
            .Sort(builder => builder.Descending(i => i.UpdatedAt))
            .Project(i => i.UpdatedAt.ToString())
            .ExecuteFirstAsync();

        var items = await _httpClient.GetFromJsonAsync<List<Item>>(_config["AuctionSvcUrl"] + "api/auctions?date=" + lastUpdated);

        return items;
    }
}

