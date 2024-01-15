using AuctionService.Data;
using Contracts;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Consumers;

public class BidPlacedConsumer : IConsumer<BidPlaced>
{
	private readonly AuctionDbContext _dbContext;

	public BidPlacedConsumer(AuctionDbContext dbContext)
	{
		_dbContext = dbContext;
	}

	public async Task Consume(ConsumeContext<BidPlaced> context)
	{

		Console.WriteLine("--> Consuming bid placed");
		var auction = await _dbContext.Auctions.FindAsync(
				Guid.Parse(context.Message.AuctionId)
				);

		if (auction.CurrentHighBid is null
				|| context.Message.BidStatus.Contains("Accepted")
				&& context.Message.Amount > auction.CurrentHighBid)
		{
			auction.CurrentHighBid = context.Message.Amount;
			await _dbContext.SaveChangesAsync();
		}

	}
}

