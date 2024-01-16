using AuctionService;
using BiddingService.Entities;
using Grpc.Net.Client;

namespace BiddingService.Services;

public class GrpcAuctionClient
{
	private readonly ILogger<GrpcAuctionClient> _logger;
	private readonly IConfiguration _config;

	public GrpcAuctionClient(ILogger<GrpcAuctionClient> logger, IConfiguration config)
	{
		_logger = logger;
		_config = config;
	}

	public Auction GetAuction(string id)
	{
		_logger.LogInformation("Calling Grpc Service");

		// channel is long-lived connection. Use as many call as possible, because
		// it is expensive operation.
		var channel = GrpcChannel.ForAddress(_config["GrpcAuction"]);
		var client = new GrpcAuction.GrpcAuctionClient(channel);

		var request = new GetAuctionRequest { Id = id };
		try
		{
			var replay = client.GetAuction(request);
			var auction = new Auction
			{
				ID = replay.Auction.Id,
				AuctionEnd = DateTime.Parse(replay.Auction.AuctionEnd),
				Seller = replay.Auction.Seller,
				ReservcePrice = replay.Auction.ReservePrice
			};

			return auction;
		}
		catch (Exception ex)
		{

			_logger.LogError(ex, "Could not call Grpc Server");
			return null;
		}
	}
}

