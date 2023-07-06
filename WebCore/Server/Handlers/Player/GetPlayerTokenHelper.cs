﻿using System.Text;
using BanHub.WebCore.Server.Context;
using BanHub.WebCore.Server.Models.Context;
using Data.Commands.Player;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BanHub.WebCore.Server.Handlers.Player;

public class GetPlayerTokenHelper : IRequestHandler<GetPlayerTokenCommand, string?>
{
    private readonly DataContext _context;

    public GetPlayerTokenHelper(DataContext context)
    {
        _context = context;
    }

    public async Task<string?> Handle(GetPlayerTokenCommand request, CancellationToken cancellationToken)
    {
        if (await _context.Players.FirstOrDefaultAsync(x => x.Identity == request.Identity, cancellationToken: cancellationToken) is not
            { } entity) return null;

        var utcTimeNow = DateTimeOffset.UtcNow;

        var hasActiveToken = await _context.AuthTokens.FirstOrDefaultAsync(x =>
            x.EntityId == entity.Id && x.Created + TimeSpan.FromMinutes(5) > utcTimeNow && !x.Used, cancellationToken: cancellationToken);
        if (hasActiveToken is not null) return hasActiveToken.Token;

        const string characters = "abcdefghjkmnpqrstuvwxyzABCDEFGHJKMNPQRSTUVWXYZ23456789";
        var result = new StringBuilder(6);
        for (var i = 0; i < 6; i++)
        {
            result.Append(characters[Random.Shared.Next(characters.Length)]);
        }

        var token = new EFAuthToken
        {
            Token = result.ToString(),
            Created = utcTimeNow,
            EntityId = entity.Id,
            Used = false
        };

        _context.AuthTokens.Add(token);
        await _context.SaveChangesAsync(cancellationToken);
        return result.ToString();
    }
}
