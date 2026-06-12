using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class LeaderboardManager : MonoBehaviour
{
    public static LeaderboardManager Instance { get; private set; }

    public float roundDurationSeconds = 180f;
    public int moneyGoal = 50;
    public int maxLeaderboardEntries = 3;
    public bool startRoundOnAwake;
    public bool endRoundWhenMoneyGoalReached = true;
    public bool useUnityGamingServices;
    public string unityLeaderboardId = "ChopCookServe";
    public string unityEnvironmentName = "production";

    public int frenchFriesValue = 10;
    public int cheesePizzaValue = 20;
    public int pepperoniPizzaValue = 25;

    private const string LeaderboardPrefsKey = "ChopCookServeLeaderboard";

    private readonly List<LeaderboardEntry> entries = new List<LeaderboardEntry>();

    private string playerName = "Player";
    private float timeRemaining;
    private int moneyEarned;
    private bool roundActive;
    private bool roundStarted;
    private bool roundEnded;
    private bool scoreSubmitted;

    public string PlayerName => playerName;
    public float TimeRemaining => timeRemaining;
    public int MoneyEarned => moneyEarned;
    public bool RoundActive => roundActive;
    public bool RoundStarted => roundStarted;
    public bool HasRoundEnded => roundEnded;
    public bool ScoreSubmitted => scoreSubmitted;
    public bool GoalReached => moneyEarned >= moneyGoal;
    public IReadOnlyList<LeaderboardEntry> Entries => entries;

    public event Action StateChanged;
    public event Action RoundEnded;

    [Serializable]
    public class LeaderboardEntry
    {
        public string playerName;
        public int money;
        public float secondsRemaining;
        public string date;
    }

    [Serializable]
    private class LeaderboardSaveData
    {
        public List<LeaderboardEntry> entries = new List<LeaderboardEntry>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadLeaderboard();

        if (startRoundOnAwake)
            StartRound(playerName);
        else
            ResetRoundState();
    }

    private void Update()
    {
        if (!roundActive)
            return;

        timeRemaining = Mathf.Max(0f, timeRemaining - Time.deltaTime);

        if (timeRemaining <= 0f)
            EndRound();

        StateChanged?.Invoke();
    }

    public void StartRound(string enteredName)
    {
        playerName = CleanPlayerName(enteredName);
        timeRemaining = roundDurationSeconds;
        moneyEarned = 0;
        roundActive = true;
        roundStarted = true;
        roundEnded = false;
        scoreSubmitted = false;
        StateChanged?.Invoke();
    }

    public void RecordCompletedOrder(OrderType orderType)
    {
        if (!roundActive)
            return;

        moneyEarned += GetOrderValue(orderType);

        if (endRoundWhenMoneyGoalReached && GoalReached)
            EndRound();
        else
            StateChanged?.Invoke();
    }

    public void EndRound()
    {
        if (!roundActive && scoreSubmitted)
            return;

        roundActive = false;
        roundEnded = true;
        Debug.Log("Leaderboard round ended. Money earned: $" + moneyEarned);
        SubmitScore();
        _ = SubmitScoreToUnityAsync();
        StateChanged?.Invoke();
        RoundEnded?.Invoke();
    }

    public void ClearLeaderboard()
    {
        entries.Clear();
        PlayerPrefs.DeleteKey(LeaderboardPrefsKey);
        PlayerPrefs.Save();
        StateChanged?.Invoke();
    }

    public void ResetCurrentRound()
    {
        ResetRoundState();
    }

    public int GetOrderValue(OrderType orderType)
    {
        switch (orderType)
        {
            case OrderType.FrenchFries:
                return frenchFriesValue;
            case OrderType.CheesePizza:
                return cheesePizzaValue;
            case OrderType.PepperoniPizza:
                return pepperoniPizzaValue;
            default:
                return 0;
        }
    }

    private void ResetRoundState()
    {
        timeRemaining = roundDurationSeconds;
        moneyEarned = 0;
        roundActive = false;
        roundStarted = false;
        roundEnded = false;
        scoreSubmitted = false;
        StateChanged?.Invoke();
    }

    private void SubmitScore()
    {
        if (scoreSubmitted || moneyEarned <= 0)
            return;

        scoreSubmitted = true;

        entries.Add(new LeaderboardEntry
        {
            playerName = playerName,
            money = moneyEarned,
            secondsRemaining = timeRemaining,
            date = DateTime.Now.ToString("yyyy-MM-dd")
        });

        SortAndTrimEntries();
        SaveLeaderboard();
    }

    private void LoadLeaderboard()
    {
        entries.Clear();

        string json = PlayerPrefs.GetString(LeaderboardPrefsKey, "");
        if (string.IsNullOrEmpty(json))
            return;

        LeaderboardSaveData saveData = JsonUtility.FromJson<LeaderboardSaveData>(json);
        if (saveData?.entries == null)
            return;

        entries.AddRange(saveData.entries);
        SortAndTrimEntries();
    }

    private void SaveLeaderboard()
    {
        LeaderboardSaveData saveData = new LeaderboardSaveData { entries = entries };
        PlayerPrefs.SetString(LeaderboardPrefsKey, JsonUtility.ToJson(saveData));
        PlayerPrefs.Save();
    }

    public async void RefreshUnityLeaderboard()
    {
        UnityLeaderboardConnector connector = GetUnityLeaderboardConnector();

        if (!useUnityGamingServices || connector == null)
            return;

        List<LeaderboardEntry> unityEntries =
            await connector.GetTopScoresAsync(maxLeaderboardEntries);

        if (unityEntries.Count == 0)
            return;

        entries.Clear();
        entries.AddRange(unityEntries);
        StateChanged?.Invoke();
    }

    private async Task SubmitScoreToUnityAsync()
    {
        UnityLeaderboardConnector connector = GetUnityLeaderboardConnector();

        if (!useUnityGamingServices)
        {
            Debug.Log("Skipping Unity leaderboard submit because Use Unity Gaming Services is unchecked.");
            return;
        }

        if (connector == null)
        {
            Debug.LogWarning("Skipping Unity leaderboard submit because no UnityLeaderboardConnector exists.");
            return;
        }

        if (moneyEarned <= 0)
        {
            Debug.Log("Skipping Unity leaderboard submit because money earned is $0.");
            return;
        }

        Debug.Log("Submitting $" + moneyEarned + " to Unity leaderboard: " + unityLeaderboardId);
        await connector.SubmitScoreAsync(moneyEarned, playerName);
        RefreshUnityLeaderboard();
    }

    private UnityLeaderboardConnector GetUnityLeaderboardConnector()
    {
        if (!useUnityGamingServices)
            return null;

        UnityLeaderboardConnector connector = UnityLeaderboardConnector.Instance;

        if (connector == null)
        {
            GameObject connectorObject = new GameObject("UnityLeaderboardConnector");
            connector = connectorObject.AddComponent<UnityLeaderboardConnector>();
        }

        connector.leaderboardId = unityLeaderboardId != null ? unityLeaderboardId.Trim() : "";
        connector.environmentName = unityEnvironmentName;
        return connector;
    }

    private void SortAndTrimEntries()
    {
        entries.Sort((a, b) =>
        {
            int moneyCompare = b.money.CompareTo(a.money);
            if (moneyCompare != 0)
                return moneyCompare;

            return b.secondsRemaining.CompareTo(a.secondsRemaining);
        });

        if (entries.Count > maxLeaderboardEntries)
            entries.RemoveRange(maxLeaderboardEntries, entries.Count - maxLeaderboardEntries);
    }

    private string CleanPlayerName(string enteredName)
    {
        string cleanName = string.IsNullOrWhiteSpace(enteredName) ? "Player" : enteredName.Trim();
        return cleanName.Length > 12 ? cleanName.Substring(0, 12) : cleanName;
    }
}
