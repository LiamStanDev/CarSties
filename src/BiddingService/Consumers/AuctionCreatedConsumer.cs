using BiddingService.Entities;
using Contracts;
using MassTransit;
using MongoDB.Entities;

namespace BiddingService.Consumers;

public class AuctionCreatedConsumer : IConsumer<AuctionCreated>
{
	public async Task Consume(ConsumeContext<AuctionCreated> context)
	{

		Console.WriteLine("--> Consuming auction created");
		var auction = new Auction
		{
			ID = context.Message.Id.ToString(),
			Seller = context.Message.Seller,
			AuctionEnd = context.Message.AuctionEnd,
			ReservcePrice = context.Message.ReservePrice
		};

		await auction.SaveAsync();
	}
}

