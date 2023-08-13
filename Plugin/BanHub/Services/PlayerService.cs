﻿using System.Net;
using System.Text.Json;
using BanHub.Configuration;
using BanHub.Interfaces;
using BanHub.Utilities;
using BanHubData.Commands.Player;
using Humanizer;
using Polly;
using Polly.Retry;
using RestEase;

namespace BanHub.Services;

public class PlayerService
{
#if DEBUG
    private const string ApiHost = "http://localhost:8123/api";
#else
    private const string ApiHost = "https://banhub.gg/api";
#endif

    private readonly IPlayerService _api;
    private readonly BanHubConfiguration _banHubConfiguration;

    private readonly AsyncRetryPolicy _retryPolicy = Policy
        .Handle<HttpRequestException>(e =>
            e.InnerException is TimeoutException || e.Message.Contains("timeout", StringComparison.OrdinalIgnoreCase))
        .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
            (exception, retryDelay, context) =>
            {
                Console.WriteLine($"[{BanHubConfiguration.Name}] Player API: {exception.Message}. " +
                                  $"Retrying in {retryDelay.Humanize()}...");
            });

    public PlayerService(BanHubConfiguration banHubConfiguration)
    {
        _banHubConfiguration = banHubConfiguration;
        _api = RestClient.For<IPlayerService>(ApiHost);
    }

    /// <summary>
    /// Get player state from BanHub master
    /// </summary>
    /// <param name="identity"></param>
    /// <returns>True if Banned, False if Not</returns>
    public async Task<bool> IsPlayerBannedAsync(IsPlayerBannedCommand identity)
    {
        try
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                var response = await _api.IsPlayerBannedAsync(_banHubConfiguration.ApiKey.ToString(), identity);
                return response.StatusCode is HttpStatusCode.Unauthorized;
            });
        }
        catch (ApiException e)
        {
            if (e.StatusCode is HttpStatusCode.Unauthorized) return true;
            Console.WriteLine($"[{BanHubConfiguration.Name}] Error getting player ban state: {e.Message}");
        }

        return false;
    }
    
    public async Task<bool> IsPlayerBannedAsync(string identity)
    {
        try
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                var response = await _api.HasIdentityBanAsync(identity);
                return response.StatusCode is HttpStatusCode.Unauthorized;
            });
        }
        catch (ApiException e)
        {
            if (e.StatusCode is HttpStatusCode.Unauthorized) return true;
            Console.WriteLine($"[{BanHubConfiguration.Name}] Error getting player ban state: {e.Message}");
        }

        return false;
    }

    public async Task<string?> GetTokenAsync(string identity)
    {
        try
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                var response = await _api.GetTokenAsync(_banHubConfiguration.ApiKey.ToString(), identity);
                if (!response.IsSuccessStatusCode) return null;
                var result = await response.Content.ReadAsStringAsync();
                return string.IsNullOrEmpty(result) ? null : result;
            });
        }
        catch (ApiException e)
        {
            var errorMessage = HelperMethods.ObscureGuid(e.Message);
            Console.WriteLine($"[{BanHubConfiguration.Name}] Error getting token: {errorMessage}");
        }

        return null;
    }

    public async Task<string?> CreateOrUpdatePlayerAsync(CreateOrUpdatePlayerCommand player)
    {
        try
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                var response = await _api.CreateOrUpdatePlayerAsync(_banHubConfiguration.ApiKey.ToString(), player);
                if (!response.IsSuccessStatusCode && _banHubConfiguration.DebugMode)
                {
                    Console.WriteLine($"\n[{BanHubConfiguration.Name}] Error posting evidence {player.PlayerIdentity}\n" +
                                      $"SC: {response.StatusCode}\n" +
                                      $"RP: {response.ReasonPhrase}\n" +
                                      $"B: {await response.Content.ReadAsStringAsync()}\n" +
                                      $"JSON: {JsonSerializer.Serialize(player)}\n" +
                                      $"[{BanHubConfiguration.Name}] End of error");
                    return null;
                }

                return await response.Content.ReadAsStringAsync();
            });
        }
        catch (ApiException e)
        {
            var errorMessage = HelperMethods.ObscureGuid(e.Message);
            Console.WriteLine($"[{BanHubConfiguration.Name}] Error updating entity: {errorMessage}");
        }

        return null;
    }
}
