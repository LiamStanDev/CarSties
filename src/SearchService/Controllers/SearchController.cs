using Microsoft.AspNetCore.Mvc;
using MongoDB.Entities;
using SearchService.Entities;
using SearchService.RequestHelper;

namespace SearchService.Controllers;

[ApiController]
[Route("api/search")]
public class SearchController : ControllerBase
{
	public async Task<ActionResult<List<Item>>> SearchItems([FromQuery] SearchParams searchParams)
	{
		var query = DB.PagedSearch<Item, Item>();


		if (!string.IsNullOrEmpty(searchParams.SearchTerm))
		{
			// the other choice is Search.Fuzzy 
			query.Match(Search.Full, searchParams.SearchTerm).SortByTextScore();
		}

		// Sorting
		query = searchParams.OrderBy switch
		{
			"make" => query.Sort(builder => builder.Ascending(i => i.Make)),
			"new" => query.Sort(builder => builder.Ascending(i => i.CreatedAt)),
			_ => query.Sort(builder => builder.Ascending(i => i.AuctionEnd))
		};

		// Filtering
		query = searchParams.FilterBy switch
		{
			"finished" => query.Match(i => i.AuctionEnd < DateTime.UtcNow),
			"endingSoon" => query.Match(i => i.AuctionEnd < DateTime.UtcNow.AddHours(6) && i.AuctionEnd > DateTime.UtcNow), // 六小時候即將結束
			_ => query.Match(i => i.AuctionEnd > DateTime.UtcNow)
		};


		if (!string.IsNullOrEmpty(searchParams.Seller))
		{
			query.Match(i => i.Seller == searchParams.Seller);
		}

		if (!string.IsNullOrEmpty(searchParams.Winner))
		{
			query.Match(i => i.Winner == searchParams.Winner);
		}

		query.PageNumber(searchParams.PageNumber);
		query.PageSize(searchParams.PageSize);

		var result = await query.ExecuteAsync();


		return Ok(new
		{
			results = result.Results,
			pageCount = result.PageCount,
			totalCount = result.TotalCount
		});
	}

}

