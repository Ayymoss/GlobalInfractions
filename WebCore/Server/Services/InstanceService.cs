﻿using BanHub.WebCore.Server.Context;
using BanHub.WebCore.Server.Enums;
using BanHub.WebCore.Server.Interfaces;
using BanHub.WebCore.Server.Models.Context;
using BanHub.WebCore.Shared.DTOs;
using Microsoft.EntityFrameworkCore;
using MudBlazor;

namespace BanHub.WebCore.Server.Services;

public class InstanceService : IInstanceService
{
    private readonly DataContext _context;
    private readonly ApiKeyCache _apiKeyCache;
    private readonly IDiscordWebhookService _discordWebhook;
    private readonly IStatisticService _statisticService;

    public InstanceService(DataContext context, ApiKeyCache apiKeyCache, IDiscordWebhookService discordWebhook,
        IStatisticService statisticService)
    {
        _context = context;
        _apiKeyCache = apiKeyCache;
        _discordWebhook = discordWebhook;
        _statisticService = statisticService;
    }

    public async Task<(ControllerEnums.ProfileReturnState, string)> CreateOrUpdate(InstanceDto request, string? requestIpAddress)
    {
        var instanceGuid = await _context.Instances
            .AsTracking()
            .FirstOrDefaultAsync(server => server.InstanceGuid == request.InstanceGuid);
        var instanceApi = await _context.Instances
            .FirstOrDefaultAsync(server => server.ApiKey == request.ApiKey);

        var ipAddress = requestIpAddress ?? request.InstanceIp;

        // New instance
        if (instanceApi is null && instanceGuid is null)
        {
            _context.Instances.Add(new EFInstance
            {
                InstanceGuid = request.InstanceGuid,
                InstanceIp = ipAddress!,
                InstanceName = request.InstanceName,
                ApiKey = request.ApiKey!.Value,
                Active = false,
                HeartBeat = DateTimeOffset.UtcNow,
                Created = DateTimeOffset.UtcNow
            });

            await _statisticService.UpdateStatistic(ControllerEnums.StatisticType.InstanceCount, ControllerEnums.StatisticTypeAction.Add);
            await _context.SaveChangesAsync();

            return (ControllerEnums.ProfileReturnState.Created, $"Instance added {request.InstanceGuid}");
        }

        // TODO: Update this... It doesn't check a mismatch...
        if (instanceGuid is null || instanceApi is null) return (ControllerEnums.ProfileReturnState.BadRequest, "GUID + API mismatch");
        if (instanceGuid.Id != instanceApi.Id)
            return (ControllerEnums.ProfileReturnState.Conflict, "Instance already exists with this API key.");

        // Warn if IP address has changed... this really shouldn't happen.
        if (requestIpAddress is not null && requestIpAddress != instanceGuid.InstanceIp)
        {
            await _discordWebhook.CreateIssueHook(instanceGuid.InstanceGuid, request.InstanceIp!, requestIpAddress);
        }

        // Update existing record
        instanceGuid.HeartBeat = DateTimeOffset.UtcNow;
        instanceGuid.InstanceName = request.InstanceName;
        _context.Instances.Update(instanceGuid);
        await _context.SaveChangesAsync();

        return instanceGuid.Active
            ? (ControllerEnums.ProfileReturnState.Accepted, "Instance exists, and is active.")
            : (ControllerEnums.ProfileReturnState.Ok, "Instance exists, but is not active.");
    }

    public async Task<(ControllerEnums.ProfileReturnState, InstanceDto?)> GetInstance(string guid)
    {
        var guidParse = Guid.TryParse(guid, out var guidResult);
        if (!guidParse) return (ControllerEnums.ProfileReturnState.BadRequest, null);

        var result = await _context.Instances.SingleOrDefaultAsync(x => x.InstanceGuid == guidResult);
        if (result is null) return (ControllerEnums.ProfileReturnState.NotFound, null);

        return (ControllerEnums.ProfileReturnState.Ok, new InstanceDto
        {
            InstanceGuid = result.InstanceGuid,
            InstanceIp = result.InstanceIp,
            InstanceName = result.InstanceName,
            Active = result.Active
        });
    }

    public async Task<List<InstanceDto>> Pagination(PaginationDto pagination)
    {
        var query = _context.Instances.AsQueryable();

        if (!string.IsNullOrWhiteSpace(pagination.SearchString))
        {
            query = query.Where(search =>
                EF.Functions.ILike(search.InstanceGuid.ToString(), $"%{pagination.SearchString}%") ||
                EF.Functions.ILike(search.InstanceName ?? "Unknown", $"%{pagination.SearchString}%") ||
                EF.Functions.ILike(search.InstanceIp, $"%{pagination.SearchString}%"));
        }

        query = pagination.SortLabel switch
        {
            "Id" => query.OrderByDirection((SortDirection)pagination.SortDirection!, key => key.Id),
            "Instance Name" => query.OrderByDirection((SortDirection)pagination.SortDirection!, key => key.InstanceName),
            "Instance IP" => query.OrderByDirection((SortDirection)pagination.SortDirection!, key => key.InstanceIp),
            "Heart Beat" => query.OrderByDirection((SortDirection)pagination.SortDirection!, key => key.HeartBeat),
            "Created" => query.OrderByDirection((SortDirection)pagination.SortDirection!, key => key.Created),
            "Servers" => query.OrderByDirection((SortDirection)pagination.SortDirection!, key => key.ServerConnections.Count),
            _ => query
        };

        var pagedData = await query
            .Skip(pagination.Page!.Value * pagination.PageSize!.Value)
            .Take(pagination.PageSize.Value)
            .Select(instance => new InstanceDto
            {
                InstanceGuid = instance.InstanceGuid,
                InstanceIp = instance.InstanceIp,
                InstanceName = instance.InstanceName,
                HeartBeat = instance.HeartBeat,
                Created = instance.Created,
                ServerCount = instance.ServerConnections.Count
            }).ToListAsync();

        return pagedData;
    }

    public async Task<ControllerEnums.ProfileReturnState> IsInstanceActive(string instanceGuid)
    {
        var guidParse = Guid.TryParse(instanceGuid, out var guidResult);
        if (!guidParse) return ControllerEnums.ProfileReturnState.BadRequest;
        var result = await _context.Instances.SingleOrDefaultAsync(x => x.InstanceGuid == guidResult);
        if (result is null) return ControllerEnums.ProfileReturnState.NotFound;

        if (result.Active && _apiKeyCache.ApiKeys is not null && !_apiKeyCache.ApiKeys.Contains(result.ApiKey))
        {
            _apiKeyCache.ApiKeys.Add(result.ApiKey);
        }

        return result.Active ? ControllerEnums.ProfileReturnState.Accepted : ControllerEnums.ProfileReturnState.Unauthorized;
    }
}
