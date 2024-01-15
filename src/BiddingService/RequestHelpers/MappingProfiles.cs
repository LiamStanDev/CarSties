using AutoMapper;
using BiddingService.DTOs;
using BiddingService.Entities;

namespace BiddingService.RequestHelpers;

public class MappingProfiles : Profile
{
	public MappingProfiles()
	{
		CreateMap<Bid, BidDto>();
	}

}

