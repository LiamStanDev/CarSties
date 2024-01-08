using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Contracts;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Controllers;

[ApiController]
[Route("api/auctions")]
public class AuctionsController : ControllerBase
{
	private readonly AuctionDbContext _context;
	private readonly IMapper _mapper;
	private readonly IPublishEndpoint _publishEndpoint;

	public AuctionsController(AuctionDbContext context, IMapper mapper, IPublishEndpoint publishEndpoint)
	{
		_context = context;
		_mapper = mapper;
		_publishEndpoint = publishEndpoint;
	}


	/// <summary>
	/// Return all auctions which update time larger than give date.
	/// </summary>
	[HttpGet]
	public async Task<ActionResult<List<AuctionDto>>> GetAllAuctions(string date)
	{

		var query = _context.Auctions.OrderBy(a => a.Item.Make).AsQueryable();

		if (!string.IsNullOrEmpty(date))
		{
			query = query.Where(a => a.UpdatedAt.CompareTo(DateTime.Parse(date).ToUniversalTime()) > 0);
		}

		// ProjectTo is automapper extension method for not to write include.
		return await query.ProjectTo<AuctionDto>(_mapper.ConfigurationProvider).ToListAsync();
	}

	[HttpGet("{id}")]
	public async Task<ActionResult<AuctionDto>> GetAuctionById(Guid id)
	{
		var auction = await _context.Auctions
			.Include(a => a.Item)
			.FirstOrDefaultAsync(a => a.Id == id); // FindAsync 無法使用 Queryable, 故沒辦法接在 Include 後


		if (auction is null)
		{
			return NotFound();
		}

		return _mapper.Map<AuctionDto>(auction);
	}

	[Authorize]
	[HttpPost]
	public async Task<ActionResult<AuctionDto>> CreateAuction(CreateAuctionDto createAuctionDto)
	{
		var newAuction = _mapper.Map<Auction>(createAuctionDto);

		newAuction.Seller = User.Identity.Name;

		// EntityFramework 會實體追蹤，會直接更新 Id
		await _context.Auctions.AddAsync(newAuction);

		var newAuctionDto = _mapper.Map<AuctionDto>(newAuction);
		// 爲什麼要將 Publish 放在 SaveChage 前面？ 因爲我們使用 OutBox，當 RabbitMQ
		// 失效，我們會在 QutBox 添加該 Message，跟其他 EFCore 操作一樣都還沒有寫入數
		// 據庫中，在 SaveChage 之後才會寫入，也就是說在 SaveChage 前面都是同一個 
		// transaction。
		// Outbox 的主要思想是將要發佈的訊息保存在數據庫中，並在數據庫事務成功提交後再
		// 將它們發佈到消息總線。以下是 Outbox 的一般流程：
		// 應用程序執行一個操作，例如保存一個新的數據庫記錄。此操作使用 Outbox 將要發佈的訊
		// 息保存到特殊的 Outbox 數據表中，同時在數據庫事務中記錄此操作。數據庫事務成功提交
		// 後，Outbox 會檢查 Outbox 數據表，並將保存的訊息發佈到消息總線。如果訊息發佈成功，
		// 則 Outbox 會將相關的 Outbox 記錄標記為已處理。如果訊息發佈失敗，Outbox 可以選擇重
		// 試操作，直到成功為止。
		await _publishEndpoint.Publish<AuctionCreated>(_mapper.Map<AuctionCreated>(newAuctionDto));

		var result = await _context.SaveChangesAsync() > 0;


		if (!result)
		{
			return BadRequest(new ProblemDetails { Title = "Could not save changes to the database" });
		}

		// 也可以使用 CreateAtRoute, 但就需要使用 Name Prop
		// Response Header 中會有 Location: http://localhost:7001/api/auctions/8d24b2f0-5168-4801-a1a3-e2693485b1cd
		return CreatedAtAction(
				nameof(GetAuctionById),
				new { newAuction.Id },
				newAuctionDto
		);
	}

	// NOTE: This feature may need to remove, because auction continue, the auction item can't change.
	[Authorize]
	[HttpPut("{id}")]
	public async Task<IActionResult> UpdateAuction(Guid id, UpdateAuctionDto updateAuctionDto)
	{
		var auction = await _context.Auctions
			.Include(a => a.Item)
			.FirstOrDefaultAsync(a => a.Id == id);

		if (auction is null)
		{
			return NotFound();
		}

		if (auction.Seller != User.Identity.Name)
		{
			return Forbid();
		}

		auction.Item.Make = updateAuctionDto.Make ?? auction.Item.Make;
		auction.Item.Model = updateAuctionDto.Model ?? auction.Item.Model;
		auction.Item.Color = updateAuctionDto.Color ?? auction.Item.Color;
		auction.Item.Year = updateAuctionDto.Year ?? auction.Item.Year;
		auction.Item.Mileage = updateAuctionDto.Mileage ?? auction.Item.Mileage;

		await _publishEndpoint.Publish<AuctionUpdated>(_mapper.Map<AuctionUpdated>(auction));

		var result = await _context.SaveChangesAsync() > 0;

		if (!result)
		{
			return BadRequest(new ProblemDetails { Title = "Could not save changes to the database" });
		}

		return Ok();
	}

	// NOTE: Delete is for admin user, because the client may want to revert their auction.
	[Authorize]
	[HttpDelete("{id}")]
	public async Task<IActionResult> DeleteAuction(Guid id)
	{
		var auction = await _context.Auctions.FindAsync(id);

		if (auction is null)
		{
			return NotFound();
		}

		if (auction.Seller != User.Identity.Name)
		{
			return Forbid();
		}


		_context.Auctions.Remove(auction);

		await _publishEndpoint.Publish<AuctionDeleted>(new { Id = auction.Id.ToString() });

		var result = await _context.SaveChangesAsync() > 0;

		if (!result)
		{
			return BadRequest(new ProblemDetails { Title = "Could not update database" });
		}

		return Ok();
	}
}

