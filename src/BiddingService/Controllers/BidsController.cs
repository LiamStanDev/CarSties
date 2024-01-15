using BiddingService.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Entities;

namespace BiddingService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BidsController : ControllerBase
{
	[Authorize]
	[HttpPost]
	public async Task<ActionResult<Bid>> PlaceBid(string auctionId, int amount)
	{
		var auction = await DB.Find<Auction>().OneAsync(auctionId);

		if (auction is null)
		{
			// TODO: ckeck with auction service if that has auction
			return NotFound();
		}

		if (auction.Seller == User.Identity.Name)
		{
			return BadRequest("You can't not bid on your own auctoin");
		}

		var bid = new Bid
		{
			Amount = amount,
			Bidder = User.Identity.Name,
			AuctionId = auctionId,
		};

		if (auction.AuctionEnd < DateTime.UtcNow)
		{
			bid.BidStatus = BidStatus.Finished;
		}
		else
		{
			var hightBid = await DB.Find<Bid>()
				.Match(b => b.AuctionId == auctionId)
				.Sort(b => b.Descending(x => x.Amount))
				.ExecuteFirstAsync();

			if (hightBid is not null && amount > hightBid.Amount || hightBid == null)
			{
				bid.BidStatus = amount > auction.ReservcePrice
					? BidStatus.Accepted
					: BidStatus.AcceptedBelowReserve;

			};

			if (hightBid is not null && bid.Amount <= hightBid.Amount)
			{
				bid.BidStatus = BidStatus.TooLow;
			}
		}


		await DB.SaveAsync(bid);

		return Ok(bid);
	}
}



