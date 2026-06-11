using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using Unity.Services.Leaderboards;
using Unity.Services.Leaderboards.Models;
using UnityEngine;

public class UnityLeaderboardConnector : MonoBehaviour
{
    public static UnityLeaderboardConnector Instance { get; private set; }

    public string leaderboardId = "e43932d2-d27c-41cd-9f30-b6c3e7921ef7";
    public string environmentName = "production";

    private bool initialized;
    private string activeEnvironmentName;

    public bool IsReady => initialized && AuthenticationService.Instance.IsSignedIn;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public async Task InitializeAsync()
    {
        if (IsReady)
            return;

        try
        {
            if (UnityServices.State == ServicesInitializationState.Uninitialized)
            {
                InitializationOptions options = new InitializationOptions();

                if (!string.IsNullOrWhiteSpace(environmentName))
                {
                    activeEnvironmentName = environmentName.Trim();
                    options.SetEnvironmentName(environmentName.Trim());
                }

                await UnityServices.InitializeAsync(options);
            }
            else
            {
                activeEnvironmentName = environmentName;
            }

            if (!AuthenticationService.Instance.IsSignedIn)
                await AuthenticationService.Instance.SignInAnonymouslyAsync();

            initialized = true;
            Debug.Log("Unity Gaming Services initialized. Project ID: " + Application.cloudProjectId +
                " | Environment: " + activeEnvironmentName +
                " | Player ID: " + AuthenticationService.Instance.PlayerId);
        }
        catch (Exception exception)
        {
            initialized = false;
            Debug.LogWarning("Could not initialize Unity Gaming Services: " + exception.Message);
        }
    }

    public async Task SubmitScoreAsync(int money, string playerName)
    {
        string cleanLeaderboardId = CleanLeaderboardId();

        if (string.IsNullOrWhiteSpace(cleanLeaderboardId))
        {
            Debug.LogWarning("Unity leaderboard ID is missing.");
            return;
        }

        await InitializeAsync();

        if (!IsReady)
            return;

        try
        {
            if (!string.IsNullOrWhiteSpace(playerName))
                await AuthenticationService.Instance.UpdatePlayerNameAsync(playerName);

            Debug.Log("Adding Unity leaderboard score. Project ID: " + Application.cloudProjectId +
                " | Environment: " + activeEnvironmentName +
                " | Leaderboard ID: '" + cleanLeaderboardId + "'" +
                " | Score: " + money);

            await LeaderboardsService.Instance.AddPlayerScoreAsync(cleanLeaderboardId, money);
            Debug.Log("Submitted $" + money + " to Unity leaderboard: " + cleanLeaderboardId);
        }
        catch (Exception exception)
        {
            Debug.LogWarning("Could not submit score to Unity leaderboard: " + exception.Message);
        }
    }

    public async Task<List<LeaderboardManager.LeaderboardEntry>> GetTopScoresAsync(int limit)
    {
        List<LeaderboardManager.LeaderboardEntry> entries = new List<LeaderboardManager.LeaderboardEntry>();
        string cleanLeaderboardId = CleanLeaderboardId();

        if (string.IsNullOrWhiteSpace(cleanLeaderboardId))
        {
            Debug.LogWarning("Unity leaderboard ID is missing.");
            return entries;
        }

        await InitializeAsync();

        if (!IsReady)
            return entries;

        try
        {
            var scores = await LeaderboardsService.Instance.GetScoresAsync(
                cleanLeaderboardId,
                new GetScoresOptions { Limit = limit }
            );

            foreach (var score in scores.Results)
            {
                entries.Add(new LeaderboardManager.LeaderboardEntry
                {
                    playerName = string.IsNullOrWhiteSpace(score.PlayerName) ? score.PlayerId : score.PlayerName,
                    money = Mathf.RoundToInt((float)score.Score),
                    secondsRemaining = 0f,
                    date = ""
                });
            }
        }
        catch (Exception exception)
        {
            Debug.LogWarning("Could not load Unity leaderboard scores: " + exception.Message);
        }

        return entries;
    }

    private string CleanLeaderboardId()
    {
        string cleanLeaderboardId = leaderboardId != null ? leaderboardId.Trim() : "";

        if (cleanLeaderboardId.Contains(" "))
            Debug.LogWarning("Unity leaderboard ID contains a space: '" + cleanLeaderboardId + "'. Use the ID, not the display name.");

        return cleanLeaderboardId;
    }
}
