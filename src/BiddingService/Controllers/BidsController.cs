using AutoMapper;
using BiddingService.DTOs;
using BiddingService.Entities;
using BiddingService.Services;
using Contracts;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Entities;

namespace BiddingService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BidsController : ControllerBase
{
	private readonly IMapper _mapper;
	private readonly IPublishEndpoint _publishEndpoint;
	private readonly GrpcAuctionClient _grpcAuctionClient;

	public BidsController(IMapper mapper, IPublishEndpoint publishEndpoint, GrpcAuctionClient grpcAuctionClient)
	{
		_mapper = mapper;
		_publishEndpoint = publishEndpoint;
		_grpcAuctionClient = grpcAuctionClient;
	}

	[Authorize]
	[HttpPost]
	public async Task<ActionResult<BidDto>> PlaceBid(string auctionId, int amount)
	{
		var auction = await DB.Find<Auction>().OneAsync(auctionId)
			?? _grpcAuctionClient.GetAuction(auctionId);


		if (auction is null)
		{
			return BadRequest("Can not accept bids on this auction at this time");
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

		// we're not worry about the transaction, because this only 
		// update for auction service currentHighBid. This property
		// is update frequently, so we don't need to ensure synchronize.
		await _publishEndpoint.Publish(_mapper.Map<BidPlaced>(bid));

		return Ok(_mapper.Map<BidDto>(bid));
	}


	[HttpGet("{auctionId}")]
	public async Task<ActionResult<List<BidDto>>> GetBidsForAuction(string auctionId)
	{
		var bids = await DB.Find<Bid>()
			.Match(a => a.AuctionId == auctionId)
			.Sort(b => b.Descending(a => a.BidTime))
			.ExecuteAsync();

		return bids.Select(b => _mapper.Map<BidDto>(b)).ToList();
	}
}



