using AutoMapper;
using Contracts;
using MassTransit;
using MongoDB.Entities;
using SearchService.Entities;

namespace SearchService.Consumers;
// 名稱結尾必須是 Consumer 這是因為 MassTransit 的 Convential-based
// 這樣可以讓我們少寫很多程式
public class AuctionCreatedConsumer : IConsumer<AuctionCreated>
{
	private readonly IMapper _mapper;

	public AuctionCreatedConsumer(IMapper mapper)
	{
		_mapper = mapper;
	}

	public async Task Consume(ConsumeContext<AuctionCreated> context)
	{
		Console.WriteLine("--> Consuming auction created: " + context.Message.Id);

		var item = _mapper.Map<Item>(context.Message);

		// this just demo how to use fault
		// if (item.Model == "Foo") throw new ArgumentException("Can't not be foo");

		await item.SaveAsync();
	}
}

