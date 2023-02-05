﻿using BanHub.WebCore.Server.Enums;
using BanHub.WebCore.Server.Interfaces;
using BanHub.WebCore.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace BanHub.WebCore.Server.Controllers;

[ApiController]
[Route("api/v2/[controller]")]
public class InstanceController : ControllerBase
{
    private readonly IInstanceService _instanceService;


    public InstanceController(IInstanceService instanceService)
    {
        _instanceService = instanceService;
    }

    /// <summary>
    /// Creates or Updates an instance.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ActionResult<string>> CreateOrUpdate([FromBody] InstanceDto request)
    {
        // This just doesn't work at all. Why!?
        //var requestIpAddress = Request.Headers["HTTP_X_FORWARDED_FOR"].ToString() ?? Request.Headers["REMOTE_ADDR"].ToString();

        var result = await _instanceService.CreateOrUpdate(request, request.InstanceIp);
        return result.Item1 switch
        {
            ControllerEnums.ProfileReturnState.Created => StatusCode(StatusCodes.Status201Created, result.Item2), // New, added
            ControllerEnums.ProfileReturnState.BadRequest => BadRequest(result.Item2), // ??
            ControllerEnums.ProfileReturnState.Conflict => StatusCode(StatusCodes.Status409Conflict, result.Item2), // Conflicting GUIDs
            ControllerEnums.ProfileReturnState.Accepted => StatusCode(StatusCodes.Status202Accepted, result.Item2), // Activated
            ControllerEnums.ProfileReturnState.Ok => Ok(result.Item2), // Not activated
            _ => BadRequest() // Should never happen
        };
    }

    [HttpGet("Active")]
    public async Task<ActionResult<bool>> IsInstanceActive([FromQuery] string guid)
    {
        var result = await _instanceService.IsInstanceActive(guid);
        return result switch
        {
            ControllerEnums.ProfileReturnState.NotFound => NotFound(),
            ControllerEnums.ProfileReturnState.BadRequest => BadRequest(),
            ControllerEnums.ProfileReturnState.Accepted => Accepted(true), // Activated
            ControllerEnums.ProfileReturnState.Unauthorized => Unauthorized(false),
            _ => BadRequest() // Should never happen
        };
    }

    [HttpGet]
    public async Task<ActionResult<InstanceDto>> GetInstance([FromQuery] string guid)
    {
        var result = await _instanceService.GetInstance(guid);
        return result.Item1 switch
        {
            ControllerEnums.ProfileReturnState.NotFound => NotFound("Instance not found"),
            ControllerEnums.ProfileReturnState.BadRequest => BadRequest("Invalid guid"),
            ControllerEnums.ProfileReturnState.Ok => Ok(result.Item2),
            _ => BadRequest() // Should never happen
        };
    }
    
    [HttpPost("All")]
    public async Task<ActionResult<IEnumerable<InstanceDto>>> GetInstances([FromBody] PaginationDto pagination)
    {
        return Ok(await _instanceService.Pagination(pagination));

    }
}
