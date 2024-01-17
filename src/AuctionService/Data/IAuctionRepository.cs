using AuctionService.DTOs;
using AuctionService.Entities;

namespace AuctionService.Data;

public interface IAuctionRepository
{
	Task<List<AuctionDto>> GetAllAuctionsAsync(string date);

	Task<Auction> GetAuctionEntityByIdAsync(Guid id);

	Task<AuctionDto> GetAuctionByIdAsync(Guid id);

	Task AddAuctionAsync(Auction auction);

	void RemoveAuction(Auction auction);

	Task<bool> SaveChangesAsync();
}

