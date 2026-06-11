using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LeaderboardUI : MonoBehaviour
{
    public TMP_InputField nameInput;
    public Button startButton;
    public Button restartButton;
    public GameObject startPanel;
    public GameObject resultsPanel;

    public TMP_Text timerText;
    public TMP_Text moneyText;
    public TMP_Text goalText;
    public TMP_Text leaderboardText;
    public TMP_Text roundMessageText;
    public bool showLeaderboardOnlyAfterRound = true;
    public string restartSceneName = "Cutscene";

    private LeaderboardManager manager;

    private void Start()
    {
        manager = LeaderboardManager.Instance;

        if (manager == null)
        {
            GameObject managerObject = new GameObject("LeaderboardManager");
            manager = managerObject.AddComponent<LeaderboardManager>();
        }

        AutoAssignMissingReferences();

        if (startButton != null)
            startButton.onClick.AddListener(StartRoundFromInput);
        else
            Debug.LogWarning("LeaderboardUI could not find StartButton.");

        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);

        manager.StateChanged += Refresh;
        manager.RoundEnded += ShowRoundEndedMessage;

        if (startPanel != null)
            startPanel.SetActive(!manager.RoundStarted);

        if (resultsPanel != null)
            resultsPanel.SetActive(manager.HasRoundEnded);

        Refresh();
    }

    private void OnDestroy()
    {
        if (manager == null)
            return;

        manager.StateChanged -= Refresh;
        manager.RoundEnded -= ShowRoundEndedMessage;
    }

    public void StartRoundFromInput()
    {
        if (manager == null)
            return;

        string enteredName = nameInput != null ? nameInput.text : "";
        manager.StartRound(enteredName);

        if (roundMessageText != null)
            roundMessageText.text = "Round started: " + manager.PlayerName;

        if (startPanel != null)
            startPanel.SetActive(false);

        if (resultsPanel != null)
            resultsPanel.SetActive(false);
    }

    public void RestartGame()
    {
        if (manager != null)
            manager.ResetCurrentRound();

        string sceneToLoad = string.IsNullOrWhiteSpace(restartSceneName) ? "Cutscene" : restartSceneName;
        SceneManager.LoadScene(sceneToLoad);
    }

    private void Refresh()
    {
        if (manager == null)
            return;

        if (timerText != null)
            timerText.text = "Time: " + FormatTime(manager.TimeRemaining);

        if (moneyText != null)
            moneyText.text = "Money: $" + manager.MoneyEarned;

        if (goalText != null)
            goalText.text = "Goal: $" + manager.moneyGoal;

        if (leaderboardText != null)
        {
            leaderboardText.text = BuildLeaderboardText();
            leaderboardText.gameObject.SetActive(!showLeaderboardOnlyAfterRound || manager.HasRoundEnded);
        }

        if (resultsPanel != null)
            resultsPanel.SetActive(manager.HasRoundEnded);

        if (startButton != null)
            startButton.interactable = !manager.RoundActive;

        if (startPanel != null && manager.RoundStarted && startPanel.activeSelf)
            startPanel.SetActive(false);
    }

    private void ShowRoundEndedMessage()
    {
        if (roundMessageText == null || manager == null)
            return;

        roundMessageText.text = manager.GoalReached
            ? "Goal reached: $" + manager.MoneyEarned
            : "Time up: $" + manager.MoneyEarned;

        Refresh();
    }

    private string BuildLeaderboardText()
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendLine("Leaderboard");

        if (manager.Entries.Count == 0)
        {
            builder.AppendLine("No scores yet");
            return builder.ToString();
        }

        for (int i = 0; i < manager.Entries.Count; i++)
        {
            LeaderboardManager.LeaderboardEntry entry = manager.Entries[i];
            builder.Append(i + 1);
            builder.Append(". ");
            builder.Append(entry.playerName);
            builder.Append("  $");
            builder.Append(entry.money);
            builder.Append("  ");
            builder.Append(FormatTime(entry.secondsRemaining));
            builder.AppendLine();
        }

        return builder.ToString();
    }

    private string FormatTime(float seconds)
    {
        int wholeSeconds = Mathf.CeilToInt(Mathf.Max(0f, seconds));
        int minutes = wholeSeconds / 60;
        int remainingSeconds = wholeSeconds % 60;
        return minutes + ":" + remainingSeconds.ToString("00");
    }

    private void AutoAssignMissingReferences()
    {
        if (nameInput == null)
            nameInput = FindSceneComponent<TMP_InputField>("NameInput");

        if (startButton == null)
            startButton = FindSceneComponent<Button>("StartButton");

        if (restartButton == null)
            restartButton = FindSceneComponent<Button>("RestartButton");

        if (startPanel == null)
            startPanel = FindSceneObject("StartPanel");

        if (resultsPanel == null)
            resultsPanel = FindSceneObject("ResultsPanel");

        if (timerText == null)
            timerText = FindSceneComponent<TMP_Text>("TimerText");

        if (moneyText == null)
            moneyText = FindSceneComponent<TMP_Text>("MoneyText");

        if (goalText == null)
            goalText = FindSceneComponent<TMP_Text>("GoalText");

        if (leaderboardText == null)
            leaderboardText = FindSceneComponent<TMP_Text>("LeaderboardText");

        if (roundMessageText == null)
            roundMessageText = FindSceneComponent<TMP_Text>("RoundMessageText");
    }

    private T FindSceneComponent<T>(string objectName) where T : Component
    {
        GameObject foundObject = FindSceneObject(objectName);
        return foundObject != null ? foundObject.GetComponent<T>() : null;
    }

    private GameObject FindSceneObject(string objectName)
    {
        GameObject foundObject = GameObject.Find(objectName);
        if (foundObject != null)
            return foundObject;

        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        for (int i = 0; i < allObjects.Length; i++)
        {
            GameObject candidate = allObjects[i];
            if (candidate.name == objectName && candidate.scene.IsValid())
                return candidate;
        }

        return null;
    }
}
