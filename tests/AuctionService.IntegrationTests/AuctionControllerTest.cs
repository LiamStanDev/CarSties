using System.Net;
using System.Net.Http.Json;
using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.IntegrationTests.Fixtures;
using AuctionService.IntegrationTests.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace AuctionService.IntegrationTests;

// IClassFixture is decorator which mean the fixture only create once
// when the first testing in this class running, then dispose when the last
// testing in this class are finish.
// Use Collection attribute instead of IClassFixture for sharing fixture.
[Collection("Shared collection")]
public class AuctionControllerTest : IAsyncLifetime
{
    private readonly CustomWebApplicationFactory _factoy;
    private readonly HttpClient _httpClient;

    // the valid GUID ID
    private const string GT_ID = "afbee524-5972-4075-8800-7d1f9d7b0a0c";

    public AuctionControllerTest(CustomWebApplicationFactory factoy)
    {
        _factoy = factoy;
        _httpClient = factoy.CreateClient();
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync()
    {
        using var scope = _factoy.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AuctionDbContext>();
        DbHelper.ReinitDbForTest(db);
        return Task.CompletedTask;
    }

    [Fact]
    public async Task GetAllAuctions_ShouldReturn3Auctions()
    {
        // arange
        // act
        var response = await _httpClient.GetFromJsonAsync<List<AuctionDto>>("api/auctions");
        // assert
        Assert.Equal(3, response.Count);
    }

    [Fact]
    public async Task GetAuctionById_WithValidId_ShoudReturnAuction()
    {
        // arange
        // act
        var response = await _httpClient.GetFromJsonAsync<AuctionDto>($"api/auctions/{GT_ID}");
        // assert
        Assert.Equal("GT", response.Model);
    }

    [Fact]
    public async Task GetAuctionById_WithInValidId_ShouldReturn404()
    {
        // arange
        // act
        var respone = await _httpClient.GetAsync($"api/auctions/{Guid.NewGuid()}");
        // assert
        Assert.Equal(HttpStatusCode.NotFound, respone.StatusCode);
    }

    [Fact]
    public async Task GetAuctionById_WithInValidId_ShouldReturn400()
    {
        // arange
        // act
        var respone = await _httpClient.GetAsync("api/auctions/notaguid");
        // assert
        Assert.Equal(HttpStatusCode.BadRequest, respone.StatusCode);
    }

    [Fact]
    public async Task CreateAuction_WithNoAuth_ShouldReturn401()
    {
        // arange
        var auction = new CreateAuctionDto
        {
            Make = "test",
        };
        // act
        var response = await _httpClient.PostAsJsonAsync("api/auctions", auction);
        // assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateAuction_WithAuth_ShouldReturn201()
    {
        // arange
        var auction = GetAuctionForCreate();
        _httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser("bob"));
        // act
        var response = await _httpClient.PostAsJsonAsync("api/auctions", auction);
        var createdAuction = await response.Content.ReadFromJsonAsync<AuctionDto>();
        // assert
        response.EnsureSuccessStatusCode(); // check 200 range code
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal("bob", createdAuction.Seller);
    }

    [Fact]
    public async Task CreateAuction_WithInValidCreateAuctionDto_ShouldReturn400()
    {
        // arange
        var auction = GetAuctionForCreate();
        auction.Make = null;
        _httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser("bob"));
        // act
        var response = await _httpClient.PostAsJsonAsync("api/auctions", auction);
        // assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateAuction_WithValidUpdateAuctionDto_ShouldReturn200()
    {
        // arange
        var updateAuction = new UpdateAuctionDto { Make = "Update" };
        _httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser("bob"));
        // act
        var response = await _httpClient.PutAsJsonAsync($"api/auctions/{GT_ID}", updateAuction);
        // assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UpdateAuction_WithValidUpdateAuctionDtoAndInValidUser_ShouldReturn403()
    {
        // arange
        var updateAuction = new UpdateAuctionDto { Make = "Update" };
        _httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser("notbob"));
        // act
        var response = await _httpClient.PutAsJsonAsync($"api/auctions/{GT_ID}", updateAuction);
        // assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
    private CreateAuctionDto GetAuctionForCreate()
    {
        return new CreateAuctionDto
        {
            Make = "test",
            Model = "testModel",
            ImageUrl = "test",
            Color = "test",
            Mileage = 10,
            Year = 10,
            ReservePrice = 10,
            AuctionEnd = DateTime.UtcNow
        };

    }
}
