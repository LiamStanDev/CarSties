using AuctionService.IntegrationTests.Fixtures;

namespace AuctionService.IntegrationTests;

// ICollectionFixture use for shared fixture for more than one test class.
[CollectionDefinition("Shared collection")]
public class SharedFixture : ICollectionFixture<CustomWebApplicationFactory>
{

}
