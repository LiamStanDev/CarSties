using MongoDB.Driver;
using MongoDB.Entities;
using SearchService.Entities;
using SearchService.Service;

namespace SearchService.Data;

public class DbInitializer
{
    public static async Task Initialize(WebApplication app)
    {
        await DB.InitAsync(
            "SearchDB",
            MongoClientSettings.FromConnectionString(
                app.Configuration["ConnectionStrings:MongoDBConnection"]
            )
        );

        // NOTE: 我們只提供三種查詢的方式
        await DB.Index<Item>()
            .Key(i => i.Make, KeyType.Text)
            .Key(i => i.Model, KeyType.Text)
            .Key(i => i.Color, KeyType.Text)
            .CreateAsync();

        // var count = await DB.CountAsync<Item>();

        // if (count == 0) {
        //     var logger = app.Services.GetRequiredService<ILogger<DbInitializer>>();
        //     logger.LogInformation("No data - will attempt to seed");
        //     var itemData = await File.ReadAllTextAsync("Data/auctions.json");
        //
        //     // 這邊的設定是要讓小寫的 json 命名規則，對應到大寫的 C# 命名規則
        //     var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        //
        //     var items = JsonSerializer.Deserialize<List<Item>>(itemData, opts);
        //
        //     await DB.SaveAsync(items);
        //
        // }
        using var scope = app.Services.CreateScope();
        var httpClient = scope.ServiceProvider.GetRequiredService<AuctionSvcHttpClient>();
        var items = await httpClient.GetItemsForSearchDB();

        Console.WriteLine(items.Count + " return from the auction service");

        if (items.Count > 0)
        {
            await DB.SaveAsync(items);
        }
    }
}
