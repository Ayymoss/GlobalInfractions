﻿using System.Net.Sockets;
using System.Text.Json;
using BanHub.Configuration;
using BanHub.Interfaces;
using BanHub.Utilities;
using BanHubData.Commands.Community.Server;
using Humanizer;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using RestEase;

namespace BanHub.Services;

public class ServerService
{
#if DEBUG
    private const string ApiHost = "http://localhost:8123/api";
#else
    private const string ApiHost = "https://banhub.gg/api";
#endif

    private readonly IServerService _api;
    private readonly BanHubConfiguration _banHubConfiguration;
    private readonly ILogger<ServerService> _logger;

    private readonly AsyncRetryPolicy _retryPolicy = Policy
        .Handle<HttpRequestException>(e => e.InnerException is SocketException)
        .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
            (exception, retryDelay, context) =>
            {
                Console.WriteLine($"[{BanHubConfiguration.Name}] Server API: {exception.Message}. " +
                                  $"Retrying in {retryDelay.Humanize()}...");
            });

    public ServerService(BanHubConfiguration banHubConfiguration, ILogger<ServerService> logger)
    {
        _banHubConfiguration = banHubConfiguration;
        _logger = logger;
        _api = RestClient.For<IServerService>(ApiHost);
    }

    public async Task<bool> PostServer(CreateOrUpdateServerCommand server)
    {
        try
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                var response = await _api.CreateOrUpdateServerAsync(_banHubConfiguration.ApiKey.ToString(), server);
                if (!response.IsSuccessStatusCode)
                {
                    var body = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Error posting server {PenaltyReason} SC: {StatusCode} RP: {ReasonPhrase} B: {Guid}",
                        server.ServerName, response.StatusCode, response.ReasonPhrase, body);
                }

                return response.IsSuccessStatusCode;
            });
        }
        catch (Exception e)
        {
            var errorMessage = HelperMethods.ObscureGuid(e.Message);
            Console.WriteLine($"[{BanHubConfiguration.Name}] Error posting server: {errorMessage}");
        }

        return false;
    }
}
