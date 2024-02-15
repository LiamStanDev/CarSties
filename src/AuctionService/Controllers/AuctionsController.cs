using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using Contracts;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace AuctionService.Controllers;

[ApiController]
[Route("api/auctions")]
public class AuctionsController : ControllerBase
{
	private readonly IAuctionRepository _repo;
	private readonly IMapper _mapper;
	private readonly IPublishEndpoint _publishEndpoint;

	public AuctionsController(IAuctionRepository repo, IMapper mapper, IPublishEndpoint publishEndpoint)
	{
		_repo = repo;
		_mapper = mapper;
		_publishEndpoint = publishEndpoint;
	}


	// <summary>
	// Return all auctions which update time larger than give date.
	// </summary>
	[HttpGet]
	public async Task<ActionResult<List<AuctionDto>>> GetAllAuctions(string date)
	{
		return await _repo.GetAllAuctionsAsync(date);

	}

	[HttpGet("{id}")]
	public async Task<ActionResult<AuctionDto>> GetAuctionById(Guid id)
	{
		var auction = await _repo.GetAuctionByIdAsync(id);

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
		await _repo.AddAuctionAsync(newAuction);

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

		var result = await _repo.SaveChangesAsync();


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

	[Authorize]
	[HttpPut("{id}")]
	public async Task<Results<Ok, BadRequest<ProblemDetails>, NotFound, ForbidHttpResult>> UpdateAuction(Guid id, UpdateAuctionDto updateAuctionDto)
	{
		var auction = await _repo.GetAuctionEntityByIdAsync(id);

		if (auction is null) { return TypedResults.NotFound(); }
		if (auction.Seller != User.Identity.Name) { return TypedResults.Forbid(); }

		auction.Update(updateAuctionDto);

		await _publishEndpoint.Publish<AuctionUpdated>(_mapper.Map<AuctionUpdated>(auction));

		var result = await _repo.SaveChangesAsync();

		if (!result)
		{
			return TypedResults.BadRequest(new ProblemDetails { Title = "Could not save changes to the database" });
		}

		return TypedResults.Ok();
	}

	// NOTE: Delete is for admin user, because the client may want to revert their auction.
	[Authorize]
	[HttpDelete("{id}")]
	public async Task<IActionResult> DeleteAuction(Guid id)
	{
		var auction = await _repo.GetAuctionEntityByIdAsync(id);

		if (auction is null)
		{
			return NotFound();
		}

		if (auction.Seller != User.Identity.Name)
		{
			return Forbid();
		}


		_repo.RemoveAuction(auction);

		await _publishEndpoint.Publish<AuctionDeleted>(new { Id = auction.Id.ToString() });

		var result = await _repo.SaveChangesAsync();

		if (!result)
		{
			return BadRequest(new ProblemDetails { Title = "Could not update database" });
		}

		return Ok();
	}
}

