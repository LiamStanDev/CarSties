using AuctionService.Controllers;
using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AuctionService.RequestHelper;
using AutoFixture;
using AutoMapper;
using MassTransit;
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

		_controller = new AuctionsController(_auctionRepo.Object, _mapper, _publishEndpoint.Object);
	}


	[Fact]
	public async Task GetAllAuctions_WithNoParams_Returns10Aucions()
	{
		// arrange
		var auctions = _fixture.CreateMany<AuctionDto>(10).ToList();
		_auctionRepo.Setup(repo => repo.GetAllAuctionsAsync(null))
			.ReturnsAsync(auctions); // async method need to use ReturnsAsync

		// act
		var results = await _controller.GetAllAuctions(null);

		// assert
		Assert.Equal(10, results.Value.Count);
		Assert.IsType<ActionResult<List<AuctionDto>>>(results);
	}

	[Fact]
	public async Task GetAuctionById_WithValidGuid_ReturnsAuction()
	{
		var auction = _fixture.Create<AuctionDto>();
		_auctionRepo.Setup(repo => repo.GetAuctionByIdAsync(auction.Id))
			.ReturnsAsync(auction);

		var result = await _controller.GetAuctionById(auction.Id);

		Assert.Equal(auction.Make, result.Value.Make);
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
