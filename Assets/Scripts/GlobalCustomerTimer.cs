using UnityEngine;
using UnityEngine.SceneManagement;

public class GlobalCustomerTimer : MonoBehaviour
{
    public static GlobalCustomerTimer Instance;

    public float minTime = 10f;
    public float maxTime = 15f;

    public string kitchenSceneName = "UpDown";

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
        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            TrySpawnOrQueueOrder();
            ResetTimer();
        }
    }

    private void ResetTimer()
    {
        timer = Random.Range(minTime, maxTime);
        Debug.Log("Next customer in " + timer + " seconds.");
    }

    private void TrySpawnOrQueueOrder()
    {
        if (SceneManager.GetActiveScene().name != kitchenSceneName)
        {
            pendingOrders++;
            Debug.Log("Customer queued while away from kitchen.");
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
            if (!TrySpawnNow())
                return;

            pendingOrders--;
        }
    }

    private bool TrySpawnNow()
    {
        OrderManager orderManager = OrderManager.Instance;

        if (orderManager == null)
            return false;

        if (!orderManager.CanSpawnNewReceipt())
            return false;

        orderManager.SpawnRandomReceipt();
        return true;
    }
}