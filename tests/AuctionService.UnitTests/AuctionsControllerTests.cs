using AuctionService.Controllers;
using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AuctionService.RequestHelper;
using AuctionService.UnitTests.Utils;
using AutoFixture;
using AutoMapper;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace AuctionService.UnitTests;

public class AuctionsControllerTests
{
	private readonly Fixture _fixture;
	// Mock can only mock interface
	// we only need to mock external service such as DB, MQ.
	private readonly Mock<IAuctionRepository> _auctionRepo;
	private readonly Mock<IPublishEndpoint> _publishEndpoint;
	// Mapper need configuration, so I don't we Mock
	private readonly IMapper _mapper;

	private readonly AuctionsController _controller;

	public AuctionsControllerTests()
	{
		_fixture = new Fixture();
		_auctionRepo = new Mock<IAuctionRepository>();
		_publishEndpoint = new Mock<IPublishEndpoint>();

		// Config AutoMapper with our mapping profile
		var mockMapperConfig = new MapperConfiguration(
			cfg => cfg.AddMaps(typeof(MappingProfile).Assembly)
		);

		_mapper = new Mapper(mockMapperConfig);

		_controller = new AuctionsController(_auctionRepo.Object, _mapper, _publishEndpoint.Object)
		{
			// Becuase we not testing the Authentication, be some method
			// need User inside HttpContext. We need to manually set.
			ControllerContext = new ControllerContext
			{
				HttpContext = new DefaultHttpContext
				{
					User = HttpContextHelper.GetClaimsPrincipal(),
				},
			}
		};
	}


	[Fact]
	public async Task GetAllAuctions_WithNoParams_Returns10Aucions()
	{
		// arrange
		var auctionsDto = _fixture.CreateMany<AuctionDto>(10).ToList();
		_auctionRepo.Setup(repo => repo.GetAllAuctionsAsync(null))
			.ReturnsAsync(auctionsDto); // async method need to use ReturnsAsync

		// act
		var results = await _controller.GetAllAuctions(null);

		// assert
		Assert.Equal(10, results.Value.Count);
		Assert.IsType<ActionResult<List<AuctionDto>>>(results);
	}

	[Fact]
	public async Task GetAuctionById_WithValidGuid_ReturnsAuction()
	{
		var auctionDto = _fixture.Create<AuctionDto>();
		_auctionRepo.Setup(repo => repo.GetAuctionByIdAsync(auctionDto.Id))
			.ReturnsAsync(auctionDto);

		var result = await _controller.GetAuctionById(auctionDto.Id);

		Assert.Equal(auctionDto.Make, result.Value.Make);
		Assert.IsType<AuctionDto>(result.Value);
	}

	[Fact]
	public async Task GetAuctionById_WithInValidGuid_ReturnsNotFound()
	{
		_auctionRepo.Setup(repo => repo.GetAuctionByIdAsync(It.IsAny<Guid>()))
			.ReturnsAsync(value: null);

		var result = await _controller.GetAuctionById(Guid.NewGuid());

		// result is type of ActionResult
		// result.Result is type of NotFound
		Assert.IsType<NotFoundResult>(result.Result);
	}

	[Fact]
	public async Task CreateAuction_WithValidCreateAuctionDto_ReturnsCreateAtActionResult()
	{
		var createAuctionDto = _fixture.Create<CreateAuctionDto>();
		_auctionRepo.Setup(repo => repo.AddAuctionAsync(It.IsAny<Auction>()));
		_auctionRepo.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(true);

		var result = await _controller.CreateAuction(createAuctionDto);
		var createdResult = result.Result as CreatedAtActionResult;

		Assert.NotNull(createdResult);
		Assert.Equal("GetAuctionById", createdResult.ActionName);
		Assert.IsType<AuctionDto>(createdResult.Value);
	}

	[Fact]
	public async Task CreateAuction_FailedSave_Returns400BadRequest()
	{
		var createAuctionDto = _fixture.Create<CreateAuctionDto>();
		_auctionRepo.Setup(repo => repo.AddAuctionAsync(It.IsAny<Auction>()));
		_auctionRepo.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(false);

		var result = await _controller.CreateAuction(createAuctionDto);

		Assert.IsType<BadRequestObjectResult>(result.Result);
	}

	[Fact]
	public async Task UpdateAuction_WithUpdateAuctionDto_ReturnsOkResponse()
	{
		// arrange
		// the auction has navigator to item and item has navigator to auction
		// so we need to create seperately
		var auction = _fixture.Build<Auction>().Without(x => x.Item).Create();
		var item = _fixture.Build<Item>().Without(x => x.Auction).Create();
		auction.Seller = "test";
		auction.Item = item;
		item.Auction = auction;
		var updateAuctionDto = _fixture.Create<UpdateAuctionDto>();
		_auctionRepo.Setup(repo => repo.GetAuctionEntityByIdAsync(It.IsAny<Guid>()))
			.ReturnsAsync(auction);
		_auctionRepo.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(true);

		// act
		var result = await _controller.UpdateAuction(auction.Id, updateAuctionDto);

		// assert
		Assert.IsType<OkResult>(result);
	}

	[Fact]
	public async Task UpdateAuction_WithInvalidUser_Returns403Forbid()
	{
		var auction = _fixture.Build<Auction>().Without(x => x.Item).Create();
		var item = _fixture.Build<Item>().Without(x => x.Auction).Create();
		auction.Item = item;
		item.Auction = auction;
		var updateAuctionDto = _fixture.Create<UpdateAuctionDto>();
		_auctionRepo.Setup(repo => repo.GetAuctionEntityByIdAsync(It.IsAny<Guid>()))
			.ReturnsAsync(auction);
		_auctionRepo.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(true);

		var result = await _controller.UpdateAuction(auction.Id, updateAuctionDto);

		Assert.IsType<ForbidResult>(result);
	}

	[Fact]
	public async Task UpdateAuction_WithInvalidGuid_ReturnsNotFound()
	{
		var auction = _fixture.Build<Auction>().Without(x => x.Item).Create();
		var item = _fixture.Build<Item>().Without(x => x.Auction).Create();
		auction.Item = item;
		item.Auction = auction;
		var updateAuctionDto = _fixture.Create<UpdateAuctionDto>();
		_auctionRepo.Setup(repo => repo.GetAuctionEntityByIdAsync(It.IsAny<Guid>()))
			.ReturnsAsync(value: null);
		_auctionRepo.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(true);

		var result = await _controller.UpdateAuction(auction.Id, updateAuctionDto);

		Assert.IsType<NotFoundResult>(result);
	}

	[Fact]
	public async Task DeleteAuction_WithValidUser_ReturnsOkResponse()
	{
		var auction = _fixture.Build<Auction>().Without(x => x.Item).Create();
		var item = _fixture.Build<Item>().Without(x => x.Auction).Create();
		auction.Seller = "test";
		_auctionRepo.Setup(repo => repo.GetAuctionEntityByIdAsync(It.IsAny<Guid>()))
			.ReturnsAsync(auction);
		_auctionRepo.Setup(repo => repo.RemoveAuction(It.IsAny<Auction>()));
		_auctionRepo.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(true);

		var result = await _controller.DeleteAuction(Guid.NewGuid());

		Assert.IsType<OkResult>(result);
	}

	[Fact]
	public async Task DeleteAuction_WithInvalidGuid_Returns404Response()
	{
		var auction = _fixture.Build<Auction>().Without(x => x.Item).Create();
		var item = _fixture.Build<Item>().Without(x => x.Auction).Create();
		auction.Seller = "test";
		_auctionRepo.Setup(repo => repo.GetAuctionEntityByIdAsync(It.IsAny<Guid>()))
			.ReturnsAsync(value: null);
		_auctionRepo.Setup(repo => repo.RemoveAuction(It.IsAny<Auction>()));
		_auctionRepo.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(true);

		var result = await _controller.DeleteAuction(Guid.NewGuid());

		Assert.IsType<NotFoundResult>(result);
	}

	[Fact]
	public async Task DeleteAuction_WithInvalidUser_Returns403Response()
	{
		var auction = _fixture.Build<Auction>().Without(x => x.Item).Create();
		var item = _fixture.Build<Item>().Without(x => x.Auction).Create();
		_auctionRepo.Setup(repo => repo.GetAuctionEntityByIdAsync(It.IsAny<Guid>()))
			.ReturnsAsync(auction);
		_auctionRepo.Setup(repo => repo.RemoveAuction(It.IsAny<Auction>()));
		_auctionRepo.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(true);

		var result = await _controller.DeleteAuction(Guid.NewGuid());

		Assert.IsType<ForbidResult>(result);
	}
}

