﻿using BanHub.WebCore.Server.Interfaces;
using Data.Domains;
using Microsoft.AspNetCore.Mvc;

namespace BanHub.WebCore.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SearchController : ControllerBase
{
    private readonly ISearchService _searchService;

    public SearchController(ISearchService searchService)
    {
        _searchService = searchService;
    }

    [HttpGet]
    public async Task<ActionResult<List<Search>>> Search([FromQuery] string query)
    {
        if (query.Length < 3) return BadRequest();
        var result = await _searchService.SearchAsync(query);
        return Ok(result);
    }
}
