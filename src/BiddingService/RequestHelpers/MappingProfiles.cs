using AutoMapper;
using BiddingService.DTOs;
using BiddingService.Entities;
using Contracts;

namespace BiddingService.RequestHelpers;

public class MappingProfiles : Profile
{
	public MappingProfiles()
	{
		CreateMap<Bid, BidDto>();
		CreateMap<Bid, BidPlaced>();
	}

}

