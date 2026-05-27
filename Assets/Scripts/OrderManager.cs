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
    public OrderState orderState;

    public float spawnInterval = 20f;
    public int maxReceiptsOnScreen = 3;
    public bool spawnReceiptsInThisScene = true;
    public bool resetOrderStateOnStart = true;
    public string kitchenSceneName = "UpDown";
    public string fridgeSceneName = "FridgeDetailed";

    private List<ReceiptOrder> receiptQueue = new List<ReceiptOrder>();
    private ReceiptOrder activeReceipt;

    private GameObject persistentReceiptCanvas;
    private RectTransform persistentReceiptParent;

    public ReceiptOrder ActiveReceipt => activeReceipt;
    public OrderState State => orderState;
    public static ReceiptOrder GlobalActiveReceipt => Instance?.activeReceipt;
    public OrderType? ActiveOrderType => activeReceipt != null ? activeReceipt.orderType : (OrderType?)null;
    public static OrderType? GlobalActiveOrderType => Instance?.activeReceipt?.orderType;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            if (_instance.orderState == null && orderState != null)
            {
                _instance.orderState = orderState;
            }

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
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        bool isFridgeScene = scene.name == fridgeSceneName;

        if (isFridgeScene)
        {
            receiptSpawnParent = persistentReceiptParent;
        }
        else
        {
            var foundLine = GameObject.Find("ReceiptLine")?.transform;
            receiptSpawnParent = foundLine != null ? foundLine : persistentReceiptParent;
        }

        ReparentPersistentReceipts();

        if (persistentReceiptCanvas != null)
            persistentReceiptCanvas.SetActive(!isFridgeScene && receiptSpawnParent == persistentReceiptParent);
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

    public void PrepareForSceneLoad()
    {
        if (persistentReceiptParent == null)
            return;

        receiptSpawnParent = persistentReceiptParent;
        ReparentPersistentReceipts();

        if (persistentReceiptCanvas != null)
            persistentReceiptCanvas.SetActive(false);

        PublishActiveOrderState();
    }

    private void Start()
    {
        if (!spawnReceiptsInThisScene)
        {
            Debug.Log("OrderManager in this scene will not spawn receipts.");
            PublishActiveOrderState();
            return;
        }

        Debug.Log("OrderManager started. Prefabs: " + (receiptPrefabs == null ? 0 : receiptPrefabs.Length));

        if (resetOrderStateOnStart && orderState != null)
        {
            orderState.Clear();
        }

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

        List<ReceiptOrder> availablePrefabs = new List<ReceiptOrder>();
        for (int i = 0; i < receiptPrefabs.Length; i++)
        {
            if (receiptPrefabs[i] == null)
            {
                Debug.LogError("Receipt prefab slot " + i + " is missing. Reassign it on the OrderManager.");
                continue;
            }

            availablePrefabs.Add(receiptPrefabs[i]);
        }

        if (availablePrefabs.Count == 0)
        {
            Debug.LogError("Cannot spawn receipt: all receipt prefab slots are missing.");
            return;
        }

        int randomIndex = Random.Range(0, availablePrefabs.Count);

        ReceiptOrder newReceipt = Instantiate(
            availablePrefabs[randomIndex],
            receiptSpawnParent
        );

        RectTransform rect = newReceipt.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.anchoredPosition = Vector2.zero;
            rect.localScale = Vector3.one * 0.6f;
        }

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

        PublishActiveOrderState();
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
            PublishActiveOrderState();
            Debug.Log("Worked on active receipt.");
        }
        else
        {
            Debug.Log("Active receipt does not need this station right now.");
        }
    }

    public bool ActiveReceiptNeedsPotatoChopping()
    {
        return activeReceipt != null &&
            activeReceipt.orderType == OrderType.FrenchFries &&
            activeReceipt.grabbedIngredients &&
            !activeReceipt.choppedOrStretched &&
            !activeReceipt.finishedOrder;
    }

    public bool CompletePotatoChoppingForActiveReceipt()
    {
        if (activeReceipt == null)
        {
            Debug.Log("No active receipt.");
            return false;
        }

        bool completedPotatoChopping = activeReceipt.CompletePotatoChopping();

        if (completedPotatoChopping)
            PublishActiveOrderState();

        return completedPotatoChopping;
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

        PublishActiveOrderState();
        Destroy(completedReceipt.gameObject);
    }

    public bool ActiveReceiptNeedsIngredient(IngredientType ingredient)
    {
        if (orderState != null && orderState.hasActiveOrder)
        {
            return orderState.ActiveOrderNeedsIngredient(ingredient);
        }

        if (activeReceipt == null)
        {
            return false;
        }

        return activeReceipt.NeedsIngredient(ingredient);
    }

    public bool ActiveReceiptUsesIngredient(IngredientType ingredient)
    {
        if (orderState != null && orderState.hasActiveOrder)
        {
            return orderState.ActiveOrderUsesIngredient(ingredient);
        }

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
            PublishActiveOrderState();
            Debug.Log("Picked correct ingredient: " + ingredient);
        }
        else
        {
            Debug.Log("Wrong ingredient: " + ingredient);
        }
    }

    public bool CompleteFridgeIngredientsForActiveReceipt()
    {
        if (activeReceipt == null)
        {
            Debug.Log("No active receipt.");
            return false;
        }

        bool completedFridgeStep = activeReceipt.TryCompleteStationTask(StationType.Fridge);
        PublishActiveOrderState();
        return completedFridgeStep;
    }

    public void PublishActiveOrderState()
    {
        if (orderState == null)
            return;

        orderState.CaptureFromReceipt(activeReceipt);
    }
}
