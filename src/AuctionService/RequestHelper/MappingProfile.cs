using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using Contracts;

namespace AuctionService.RequestHelper;

public class MappingProfile : Profile
{
	public MappingProfile()
	{
		CreateMap<Auction, AuctionDto>().IncludeMembers(e => e.Item); // Auction has a navigator to item
		CreateMap<Item, AuctionDto>(); // Item has a navigator to auction 

		CreateMap<CreateAuctionDto, Auction>()
			// d stard for destination, s standard for source
			.ForMember(d => d.Item, memOpts => memOpts.MapFrom(s => s)); // CreateAuction has props for Auction's Item
		CreateMap<CreateAuctionDto, Item>();
		CreateMap<AuctionDto, AuctionCreated>();

		CreateMap<Auction, AuctionUpdated>().IncludeMembers(e => e.Item);
		CreateMap<Item, AuctionUpdated>(); // 因爲上面有 includemembers 的 item
	}
}
