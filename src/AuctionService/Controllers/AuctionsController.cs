using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Controllers;

[ApiController]
[Route("api/auctions")]
public class AuctionsController : ControllerBase {
    private readonly AuctionDbContext _context;
    private readonly IMapper _mapper;

    public AuctionsController(AuctionDbContext context, IMapper mapper) {
        _context = context;
        _mapper = mapper;
    }


    [HttpGet]
    public async Task<ActionResult<List<AuctionDto>>> GetAllAuctions() {


        var auctions = await _context.Auctions.AsQueryable()
            .Include(a => a.Item)
            .OrderBy(a => a.Item.Make)
            .ToListAsync();

        return _mapper.Map<List<AuctionDto>>(auctions);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AuctionDto>> GetAuctionById(Guid id) {
        var auction = await _context.Auctions
            .Include(a => a.Item)
            .FirstOrDefaultAsync(a => a.Id == id); // FindAsync 無法使用 Queryable, 故沒辦法接在 Include 後


        if (auction is null) {
            return NotFound();
        }

        return _mapper.Map<AuctionDto>(auction);
    }

    [HttpPost]
    public async Task<ActionResult<AuctionDto>> CreateAuction(CreateAuctionDto createAuctionDto) {
        var newAuction = _mapper.Map<Auction>(createAuctionDto);

        // TODO: add current user as seller
        newAuction.Seller = "test";

        await _context.Auctions.AddAsync(newAuction);

        var result = await _context.SaveChangesAsync() > 0;

        if (!result) {
            return BadRequest(new ProblemDetails { Title = "Could not save changes to the database" });
        }

        // 也可以使用 CreateAtRoute, 但就需要使用 Name Prop
        // Response Header 中會有 Location: http://localhost:7001/api/auctions/8d24b2f0-5168-4801-a1a3-e2693485b1cd
        return CreatedAtAction(
                nameof(GetAuctionById),
                new { newAuction.Id },
                _mapper.Map<AuctionDto>(newAuction)
        );
    }

    // NOTE: This feature may need to remove, because auction continue, the auction item can't change.
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAuction(Guid id, UpdateAuctionDto updateAuctionDto) {
        var auction = await _context.Auctions
            .Include(a => a.Item)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (auction is null) {
            return NotFound();
        }

        // TODO: check seller == username

        auction.Item.Make = updateAuctionDto.Make ?? auction.Item.Make;
        auction.Item.Model = updateAuctionDto.Model ?? auction.Item.Model;
        auction.Item.Color = updateAuctionDto.Color ?? auction.Item.Color;
        auction.Item.Year = updateAuctionDto.Year ?? auction.Item.Year;
        auction.Item.Mileage = updateAuctionDto.Mileage ?? auction.Item.Mileage;


        var result = await _context.SaveChangesAsync() > 0;

        if (!result) {
            return BadRequest(new ProblemDetails { Title = "Could not save changes to the database" });
        }

        return Ok();
    }

    // NOTE: Delete is for admin user, because the client may want to revert their auction.
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAuction(Guid id) {
        var auction = await _context.Auctions.FindAsync(id);

        if (auction is null) {
            return NotFound();
        }

        // TODO: check auction.Seller == username

        _context.Auctions.Remove(auction);

        var result = await _context.SaveChangesAsync() > 0;

        if (!result) {
            return BadRequest(new ProblemDetails { Title = "Could not update database" });
        }

        return Ok();
    }
}

