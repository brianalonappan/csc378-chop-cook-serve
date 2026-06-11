using UnityEngine;
using UnityEngine.SceneManagement;

public class GlobalCustomerTimer : MonoBehaviour
{
    public static GlobalCustomerTimer Instance;

    public float minTime = 10f;
    public float maxTime = 15f;
    public string kitchenSceneName = "UpDown";
    public bool requireActiveLeaderboardRound;

    private float timer;
    private int pendingOrders;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
        ResetTimer();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Update()
    {
        if (requireActiveLeaderboardRound &&
            LeaderboardManager.Instance != null &&
            !LeaderboardManager.Instance.RoundActive)
        {
            ResetTimer();
            return;
        }

        if (CustomerOrOrderIsActive())
        {
            ResetTimer();
            return;
        }

        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            TrySpawnOrQueueOrder();
            ResetTimer();
        }
    }

    private bool CustomerOrOrderIsActive()
    {
        OrderManager orderManager = OrderManager.Instance;

        if (orderManager == null || orderManager.customerObject == null)
            return false;

        CustomerWalker walker = orderManager.customerObject.GetComponent<CustomerWalker>();

        return walker != null && walker.IsBusyWithCustomer();
    }

    private void ResetTimer()
    {
        timer = Random.Range(minTime, maxTime);
    }

    private void TrySpawnOrQueueOrder()
    {
        if (SceneManager.GetActiveScene().name != kitchenSceneName)
        {
            if (!CustomerOrOrderIsActive())
                pendingOrders++;

            return;
        }

        TrySpawnNow();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != kitchenSceneName)
            return;

        while (pendingOrders > 0)
        {
            if (CustomerOrOrderIsActive())
                return;

            TrySpawnNow();
            pendingOrders--;
        }
    }

    private void TrySpawnNow()
    {
        OrderManager orderManager = OrderManager.Instance;

        if (orderManager == null)
            return;

        if (CustomerOrOrderIsActive())
            return;

        orderManager.SpawnRandomReceipt();
    }
}
