using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class OrderManager : MonoBehaviour
{
    private static OrderManager _instance;
    public static OrderManager Instance
    {
        get
        {
            if (_instance == null)
                _instance = FindAnyObjectByType<OrderManager>();

            return _instance;
        }
    }

    public ReceiptOrder[] receiptPrefabs;
    public Transform receiptSpawnParent; // drag ReceiptLine here

    public float spawnInterval = 20f;
    public int maxReceiptsOnScreen = 3;
    public bool spawnReceiptsInThisScene = true;

    private List<ReceiptOrder> receiptQueue = new List<ReceiptOrder>();
    private ReceiptOrder activeReceipt;

    private GameObject persistentReceiptCanvas;
    private RectTransform persistentReceiptParent;

    public ReceiptOrder ActiveReceipt => activeReceipt;
    public static ReceiptOrder GlobalActiveReceipt => Instance?.activeReceipt;
    public OrderType? ActiveOrderType => activeReceipt != null ? activeReceipt.orderType : (OrderType?)null;
    public static OrderType? GlobalActiveOrderType => Instance?.activeReceipt?.orderType;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            if (_instance.receiptSpawnParent == null && receiptSpawnParent != null)
            {
                _instance.receiptSpawnParent = receiptSpawnParent;
                Debug.Log("Transferred Receipt Spawn Parent to existing OrderManager instance.");
            }

            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
        CreatePersistentReceiptCanvas();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        if (_instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        var foundLine = GameObject.Find("ReceiptLine")?.transform;
        if (foundLine != null)
        {
            receiptSpawnParent = foundLine;
        }
        // Don't hide canvas just because this scene has no ReceiptLine
        // The persistent canvas stays visible always
    }

    private void CreatePersistentReceiptCanvas()
    {
        if (persistentReceiptCanvas != null)
            return;

        persistentReceiptCanvas = new GameObject("PersistentReceiptCanvas");
        Canvas canvas = persistentReceiptCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        persistentReceiptCanvas.AddComponent<CanvasScaler>();
        persistentReceiptCanvas.AddComponent<GraphicRaycaster>();
        DontDestroyOnLoad(persistentReceiptCanvas);

        GameObject receiptLine = new GameObject("ReceiptLine");
        receiptLine.transform.SetParent(persistentReceiptCanvas.transform, false);
        persistentReceiptParent = receiptLine.AddComponent<RectTransform>();
        persistentReceiptParent.anchorMin = new Vector2(0f, 1f);
        persistentReceiptParent.anchorMax = new Vector2(0f, 1f);
        persistentReceiptParent.pivot = new Vector2(0f, 1f);
        persistentReceiptParent.anchoredPosition = new Vector2(50f, -50f);
        persistentReceiptParent.sizeDelta = new Vector2(800f, 200f);
        persistentReceiptCanvas.SetActive(false);
    }

    private void ReparentPersistentReceipts()
    {
        if (receiptSpawnParent == null)
            return;

        foreach (ReceiptOrder receipt in receiptQueue)
        {
            if (receipt == null)
                continue;

            if (receipt.transform.parent != receiptSpawnParent)
            {
                receipt.transform.SetParent(receiptSpawnParent, false);
                RectTransform rect = receipt.GetComponent<RectTransform>();
                if (rect != null)
                {
                    rect.anchoredPosition = Vector2.zero;
                    rect.localScale = Vector3.one * 0.6f;
                }
            }
        }
    }

    private void Start()
    {
        if (!spawnReceiptsInThisScene)
        {
            Debug.Log("OrderManager in this scene will not spawn receipts.");
            return;
        }

        Debug.Log("OrderManager started. Prefabs: " + (receiptPrefabs == null ? 0 : receiptPrefabs.Length));

        if (receiptSpawnParent == null)
        {
            receiptSpawnParent = GameObject.Find("ReceiptLine")?.transform ?? persistentReceiptParent;
            if (receiptSpawnParent != null && receiptSpawnParent != persistentReceiptParent)
            {
                Debug.Log("Found scene ReceiptLine at runtime and assigned receiptSpawnParent.");
            }
            else if (receiptSpawnParent == persistentReceiptParent)
            {
                Debug.Log("Using persistent receipt canvas because no scene ReceiptLine was found.");
            }
        }

        if (persistentReceiptCanvas != null)
            persistentReceiptCanvas.SetActive(receiptSpawnParent == persistentReceiptParent);

        ReparentPersistentReceipts();

        if (receiptPrefabs == null || receiptPrefabs.Length == 0)
        {
            Debug.LogError("No receiptPrefabs assigned! Add at least one ReceiptOrder prefab.");
            return;
        }

        SpawnRandomReceipt();
        StartCoroutine(SpawnReceiptRoutine());
    }

    private IEnumerator SpawnReceiptRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            if (receiptQueue.Count < maxReceiptsOnScreen)
            {
                SpawnRandomReceipt();
            }
        }
    }

    private void SpawnRandomReceipt()
    {
        if (receiptPrefabs == null || receiptPrefabs.Length == 0)
        {
            Debug.LogError("Cannot spawn receipt: receiptPrefabs is empty.");
            return;
        }

        if (receiptSpawnParent == null)
        {
            Debug.LogError("Cannot spawn receipt: receiptSpawnParent is missing.");
            return;
        }

        int randomIndex = Random.Range(0, receiptPrefabs.Length);

        ReceiptOrder newReceipt = Instantiate(
            receiptPrefabs[randomIndex],
            receiptSpawnParent
        );

        RectTransform rect = newReceipt.GetComponent<RectTransform>();
        rect.anchoredPosition = Vector2.zero;
        rect.localScale = Vector3.one * 0.6f;

        receiptQueue.Add(newReceipt);
        newReceipt.SetHighlighted(false);

        if (activeReceipt == null)
        {
            SetActiveReceipt(newReceipt);
        }
    }

    public void SetActiveReceipt(ReceiptOrder receipt)
    {
        if (activeReceipt != null)
            activeReceipt.SetHighlighted(false);

        activeReceipt = receipt;

        if (activeReceipt != null)
            activeReceipt.SetHighlighted(true);

        Debug.Log(activeReceipt != null
            ? activeReceipt.orderType + " is now active."
            : "Active receipt cleared.");
    }

    public void TryUseStation(StationType stationType)
    {
        if (activeReceipt == null)
        {
            Debug.Log("No active receipt.");
            return;
        }

        bool completedSomething =
            activeReceipt.TryCompleteStationTask(stationType);

        if (completedSomething)
        {
            Debug.Log("Worked on active receipt.");
        }
        else
        {
            Debug.Log("Active receipt does not need this station right now.");
        }
    }

    public void CompleteReceipt(ReceiptOrder completedReceipt)
    {
        if (receiptQueue.Contains(completedReceipt))
        {
            receiptQueue.Remove(completedReceipt);
        }

        if (activeReceipt == completedReceipt)
        {
            activeReceipt.SetHighlighted(false);
            activeReceipt = receiptQueue.Count > 0 ? receiptQueue[0] : null;

            if (activeReceipt != null)
                activeReceipt.SetHighlighted(true);
        }

        Destroy(completedReceipt.gameObject);
    }

    public bool ActiveReceiptNeedsIngredient(IngredientType ingredient)
    {
        if (activeReceipt == null)
        {
            return false;
        }

        return activeReceipt.NeedsIngredient(ingredient);
    }

    public bool ActiveReceiptUsesIngredient(IngredientType ingredient)
    {
        if (activeReceipt == null)
        {
            return false;
        }

        return activeReceipt.ContainsFridgeIngredient(ingredient);
    }

    public void PickIngredientForActiveReceipt(IngredientType ingredient)
    {
        if (activeReceipt == null)
        {
            Debug.Log("No active receipt.");
            return;
        }

        bool pickedCorrectIngredient = activeReceipt.TryPickIngredient(ingredient);

        if (pickedCorrectIngredient)
        {
            Debug.Log("Picked correct ingredient: " + ingredient);
        }
        else
        {
            Debug.Log("Wrong ingredient: " + ingredient);
        }
}
}