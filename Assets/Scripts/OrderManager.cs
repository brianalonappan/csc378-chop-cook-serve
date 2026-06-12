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
    public Transform receiptSpawnParent;
    public OrderState orderState;

    public float spawnInterval = 20f;
    public int maxReceiptsOnScreen = 3;
    public bool spawnReceiptsInThisScene = true;
    public bool resetOrderStateOnStart = true;

    public string kitchenSceneName = "UpDown";
    public string fridgeSceneName = "FridgeDetailed";
    public string potatoCuttingSceneName = "CuttingBoardPotato";
    public string potatoMixSceneName = "MixPotato";
    public string fryingSceneName = "FryingScene";
    public string cheesePizzaSceneName = "CheeseScene";
    public string pepperoniPizzaSceneName = "PepperoniScene";
    public string pizzaPrepSceneName = "PizzaPrepScene";
    public string cheeseOvenSceneName = "CheeseOvenScene";
    public string pepperoniOvenSceneName = "PepperoniOvenScene";

    public bool showReceiptOnlyAfterCashier = true;
    public GameObject customerObject;

    private List<ReceiptOrder> receiptQueue = new List<ReceiptOrder>();
    private ReceiptOrder activeReceipt;

    private GameObject persistentReceiptCanvas;
    private RectTransform persistentReceiptParent;

    private bool hasPlayerReturnPosition;
    private Vector3 playerReturnPosition;

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
                _instance.orderState = orderState;

            if (_instance.receiptSpawnParent == null && receiptSpawnParent != null)
                _instance.receiptSpawnParent = receiptSpawnParent;

            if (_instance.customerObject == null && customerObject != null)
                _instance.customerObject = customerObject;

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

    private void Start()
    {
        StartCoroutine(DelayedStart());
    }

    private IEnumerator DelayedStart()
    {
        Debug.Log("OrderManager waiting 1 second before starting...");

        yield return new WaitForSeconds(1f);

        if (!spawnReceiptsInThisScene)
        {
            Debug.Log("OrderManager in this scene will not spawn receipts.");
            PublishActiveOrderState();
            yield break;
        }

        if (resetOrderStateOnStart && orderState != null)
            orderState.Clear();

        if (receiptSpawnParent == null)
            receiptSpawnParent = GameObject.Find("ReceiptLine")?.transform ?? persistentReceiptParent;

        if (persistentReceiptCanvas != null)
        {
            persistentReceiptCanvas.SetActive(
                !ShouldHideReceiptsInScene(SceneManager.GetActiveScene().name) &&
                receiptSpawnParent == persistentReceiptParent
            );
        }

        ReparentPersistentReceipts();

        if (receiptPrefabs == null || receiptPrefabs.Length == 0)
        {
            Debug.LogError("No receiptPrefabs assigned! Add at least one ReceiptOrder prefab.");
            yield break;
        }

        if (receiptSpawnParent == null)
        {
            Debug.LogError("Cannot spawn receipt: receiptSpawnParent is missing.");
            yield break;
        }

        Debug.Log("OrderManager finished delayed setup.");
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        bool shouldHideReceipts = ShouldHideReceiptsInScene(scene.name);

        if (shouldHideReceipts)
        {
            receiptSpawnParent = persistentReceiptParent;
        }
        else
        {
            Transform foundLine = GameObject.Find("ReceiptLine")?.transform;
            receiptSpawnParent = foundLine != null ? foundLine : persistentReceiptParent;
        }

        ReparentPersistentReceipts();

        if (persistentReceiptCanvas != null)
        {
            persistentReceiptCanvas.SetActive(
                !shouldHideReceipts &&
                receiptSpawnParent == persistentReceiptParent
            );
        }

        if (scene.name == kitchenSceneName)
        {
            StartCoroutine(RestorePlayerReturnPositionAfterSceneLoad());
            StartCoroutine(RestoreCustomerAfterKitchenLoad());
        }
    }

    private IEnumerator RestoreCustomerAfterKitchenLoad()
    {
        yield return null;

        if (customerObject == null)
            yield break;

        CustomerWalker walker = customerObject.GetComponent<CustomerWalker>();

        if (walker == null)
            yield break;

        if (activeReceipt != null && activeReceipt.orderReceived && !activeReceipt.finishedOrder)
        {
            walker.RestoreWaitingCustomer();
        }
    }

    public void SavePlayerReturnPosition(Vector3 position)
    {
        playerReturnPosition = position;
        hasPlayerReturnPosition = true;
    }

    private IEnumerator RestorePlayerReturnPositionAfterSceneLoad()
    {
        if (!hasPlayerReturnPosition)
            yield break;

        yield return null;

        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player == null)
        {
            Debug.LogWarning("Could not restore player position because no object is tagged Player.");
            yield break;
        }

        player.transform.position = playerReturnPosition;

        Rigidbody2D playerBody = player.GetComponent<Rigidbody2D>();
        if (playerBody != null)
            playerBody.position = playerReturnPosition;

        hasPlayerReturnPosition = false;
    }

    private bool ShouldHideReceiptsInScene(string sceneName)
    {
        return sceneName == fridgeSceneName ||
               sceneName == potatoCuttingSceneName ||
               sceneName == potatoMixSceneName ||
               sceneName == fryingSceneName ||
               sceneName == cheesePizzaSceneName ||
               sceneName == pepperoniPizzaSceneName ||
               sceneName == pizzaPrepSceneName ||
               sceneName == cheeseOvenSceneName ||
               sceneName == pepperoniOvenSceneName;
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

    private IEnumerator SpawnReceiptRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            if (CanSpawnNewReceipt())
                SpawnRandomReceipt();
        }
    }

    public void SpawnRandomReceipt()
    {
        if (!CanSpawnNewReceipt())
            return;

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
            if (receiptPrefabs[i] != null)
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

        if (showReceiptOnlyAfterCashier)
            newReceipt.gameObject.SetActive(false);

        if (activeReceipt == null)
            SetActiveReceipt(newReceipt);

        StartCustomerWalkIn();
    }

    private void StartCustomerWalkIn()
    {
        if (customerObject == null)
            return;

        CustomerWalker walker = customerObject.GetComponent<CustomerWalker>();

        if (walker != null)
            walker.StartNewCustomer();
        else
            customerObject.SetActive(true);
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

        if (stationType == StationType.Cashier)
        {
            if (!CustomerIsAtRegister())
            {
                Debug.Log("Cannot use register yet. Customer has not reached the register.");
                return;
            }
        }

        bool completedSomething = activeReceipt.TryCompleteStationTask(stationType);

        if (completedSomething && stationType == StationType.Cashier)
        {
            activeReceipt.gameObject.SetActive(true);
            SendCustomerToWaitPoint();
        }

        if (completedSomething && stationType == StationType.DropOff)
        {
            SendCustomerOut();
        }

        if (completedSomething && stationType == StationType.TrashCan)
        {
            SendCustomerOut();
        }

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

    private bool CustomerIsAtRegister()
    {
        if (customerObject == null)
            return false;

        CustomerWalker walker = customerObject.GetComponent<CustomerWalker>();

        return walker != null && walker.reachedRegister;
    }

    private void SendCustomerToWaitPoint()
    {
        if (customerObject == null)
            return;

        CustomerWalker walker = customerObject.GetComponent<CustomerWalker>();

        if (walker != null)
            walker.WalkToWaitPoint();
    }

    private void SendCustomerOut()
    {
        if (customerObject == null)
            return;

        CustomerWalker walker = customerObject.GetComponent<CustomerWalker>();

        if (walker != null)
            walker.LeaveRestaurant();
    }

    public bool ActiveReceiptNeedsPotatoChopping()
    {
        return activeReceipt != null &&
               activeReceipt.orderType == OrderType.FrenchFries &&
               activeReceipt.grabbedIngredients &&
               !activeReceipt.choppedOrStretched &&
               !activeReceipt.finishedOrder;
    }

    public bool ActiveReceiptNeedsPotatoMixing()
    {
        return activeReceipt != null &&
               activeReceipt.orderType == OrderType.FrenchFries &&
               activeReceipt.cookedOrBaked &&
               !activeReceipt.addedToppings &&
               !activeReceipt.finishedOrder;
    }

    public bool ActiveReceiptNeedsFriesFrying()
    {
        return activeReceipt != null &&
               activeReceipt.orderType == OrderType.FrenchFries &&
               activeReceipt.choppedOrStretched &&
               !activeReceipt.cookedOrBaked &&
               !activeReceipt.finishedOrder;
    }

    public bool ActiveReceiptNeedsPizzaPrep()
    {
        return activeReceipt != null &&
               (activeReceipt.orderType == OrderType.CheesePizza ||
                activeReceipt.orderType == OrderType.PepperoniPizza) &&
               activeReceipt.grabbedIngredients &&
               !activeReceipt.addedToppings &&
               !activeReceipt.finishedOrder;
    }

    public bool ActiveReceiptNeedsPizzaBaking()
    {
        return activeReceipt != null &&
               (activeReceipt.orderType == OrderType.CheesePizza ||
                activeReceipt.orderType == OrderType.PepperoniPizza) &&
               activeReceipt.addedToppings &&
               !activeReceipt.cookedOrBaked &&
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

    public bool CompletePotatoMixingForActiveReceipt()
    {
        if (activeReceipt == null)
        {
            Debug.Log("No active receipt.");
            return false;
        }

        bool completedPotatoMixing = activeReceipt.CompletePotatoMixing();

        if (completedPotatoMixing)
            PublishActiveOrderState();

        return completedPotatoMixing;
    }

    public bool CompleteFriesFryingForActiveReceipt(bool burned)
    {
        if (activeReceipt == null)
        {
            Debug.Log("No active receipt.");
            return false;
        }

        bool completedFriesFrying = activeReceipt.CompleteFriesFrying(burned);

        if (completedFriesFrying)
            PublishActiveOrderState();

        return completedFriesFrying;
    }

    public bool CompletePizzaPrepForActiveReceipt()
    {
        if (activeReceipt == null)
        {
            Debug.Log("No active receipt.");
            return false;
        }

        bool completedPizzaPrep = activeReceipt.CompletePizzaPrep();

        if (completedPizzaPrep)
            PublishActiveOrderState();

        return completedPizzaPrep;
    }

    public void CompleteReceipt(ReceiptOrder completedReceipt)
    {
        if (receiptQueue.Contains(completedReceipt))
            receiptQueue.Remove(completedReceipt);

        if (activeReceipt == completedReceipt)
        {
            activeReceipt.SetHighlighted(false);
            activeReceipt = receiptQueue.Count > 0 ? receiptQueue[0] : null;

            if (activeReceipt != null)
                activeReceipt.SetHighlighted(true);
        }

        PublishActiveOrderState();

        if (completedReceipt != null)
            Destroy(completedReceipt.gameObject);
    }

    public bool ActiveReceiptNeedsIngredient(IngredientType ingredient)
    {
        if (orderState != null && orderState.hasActiveOrder)
            return orderState.ActiveOrderNeedsIngredient(ingredient);

        if (activeReceipt == null)
            return false;

        return activeReceipt.NeedsIngredient(ingredient);
    }

    public bool ActiveReceiptUsesIngredient(IngredientType ingredient)
    {
        if (orderState != null && orderState.hasActiveOrder)
            return orderState.ActiveOrderUsesIngredient(ingredient);

        if (activeReceipt == null)
            return false;

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

    public bool CanSpawnNewReceipt()
    {
        return receiptQueue.Count < maxReceiptsOnScreen;
    }
}