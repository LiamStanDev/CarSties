using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Data;

public class AuctionRepository : IAuctionRepository
{
	private readonly AuctionDbContext _context;
	private readonly IMapper _mapper;

	public AuctionRepository(AuctionDbContext context, IMapper mapper)
	{
		_context = context;
		_mapper = mapper;
	}

	public async Task AddAuctionAsync(Auction auction)
	{
		await _context.Auctions.AddAsync(auction);
	}

	public async Task<List<AuctionDto>> GetAllAuctionsAsync(string date)
	{
		var query = _context.Auctions.OrderBy(a => a.Item.Make).AsQueryable();

		if (!string.IsNullOrEmpty(date))
		{
			query = query.Where(a => a.UpdatedAt.CompareTo(DateTime.Parse(date).ToUniversalTime()) > 0);
		}

		// ProjectTo is automapper extension method for not to write include.
		return await query.ProjectTo<AuctionDto>(_mapper.ConfigurationProvider).ToListAsync();
	}

	public async Task<AuctionDto> GetAuctionByIdAsync(Guid id)
	{
		return await _context.Auctions
			.ProjectTo<AuctionDto>(_mapper.ConfigurationProvider)
			.FirstOrDefaultAsync(a => a.Id == id);
	}

	public async Task<Auction> GetAuctionEntityByIdAsync(Guid id)
	{
		return await _context.Auctions
			.Include(a => a.Item)
			.FirstOrDefaultAsync(a => a.Id == id);
	}

	public void RemoveAuction(Auction auction)
	{
		_context.Auctions.Remove(auction);
	}

	public async Task<bool> SaveChangesAsync()
	{
		return await _context.SaveChangesAsync() > 0;
	}
}

