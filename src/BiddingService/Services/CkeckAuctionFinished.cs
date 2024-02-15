
using BiddingService.Entities;
using Contracts;
using MassTransit;
using MongoDB.Entities;

namespace BiddingService.Services;


// BackgroundSerivce is Singleton lifetime, so we can't
// use DI to inject scope lifetime.
public class CheckAuctionFinished : BackgroundService
{
	private readonly ILogger<CheckAuctionFinished> _logger;
	private readonly IServiceProvider _services;

	// ILogger, IServiceProvider are Singleton
	public CheckAuctionFinished(ILogger<CheckAuctionFinished> logger, IServiceProvider services)
	{
		_logger = logger;
		_services = services;
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		_logger.LogInformation("Starting check for finished auctions");

		stoppingToken.Register(() => _logger.LogInformation("===> Auction ckeck is stopping"));

		while (!stoppingToken.IsCancellationRequested)
		{
			await CheckAuctions(stoppingToken);

			await Task.Delay(5000);
		}
	}

	private async Task CheckAuctions(CancellationToken stoppingToken)
	{
		var finishedAuctions = await DB.Find<Auction>()
			.Match(x => x.AuctionEnd <= DateTime.UtcNow)
			.Match(x => !x.Finished)
			.ExecuteAsync(stoppingToken);

		if (finishedAuctions.Count == 0)
		{
			return;
		}

		_logger.LogInformation("==> Found {count} auctions that have complete", finishedAuctions.Count);

		using var scope = _services.CreateScope();

		var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

		foreach (var auction in finishedAuctions)
		{
			auction.Finished = true;
			await auction.SaveAsync(null, stoppingToken);

			var winningBid = await DB.Find<Bid>()
				.Match(a => a.AuctionId == auction.ID)
				.Match(a => a.BidStatus == BidStatus.Accepted)
				.Sort(x => x.Descending(a => a.Amount))
				.ExecuteFirstAsync(stoppingToken);

			await publishEndpoint.Publish(new AuctionFinished
			{
				ItemSold = winningBid is not null,
				// I manually keep it optional, to check this have no winningBid.
				AuctionId = auction.ID,
				Winner = winningBid?.Bidder,
				Amount = winningBid?.Amount,
				Seller = auction.Seller
			}, stoppingToken);
		}
	}
}

