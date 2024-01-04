using AutoMapper;
using Contracts;
using SearchService.Entities;

namespace SearchService.RequestHelper;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        CreateMap<AuctionCreated, Item>();
        CreateMap<AuctionDeleted, Item>();
        CreateMap<AuctionUpdated, Item>();
    }
}

