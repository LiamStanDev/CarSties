using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;

namespace AuctionService.RequestHelper;

public class MappingProfile : Profile
{

	public MappingProfile()
	{
		CreateMap<Auction, AuctionDto>()
			.IncludeMembers(auction => auction.Item); // Auction has Item to map

		CreateMap<Item, AuctionDto>();

		CreateMap<CreateAuctionDto, Auction>()
			// d stard for destination, s standard for source
			.ForMember(d => d.Item, memOpts => memOpts.MapFrom(s => s)); // CreateAuction has props for Auction's Item
		CreateMap<CreateAuctionDto, Item>();
	}


}

