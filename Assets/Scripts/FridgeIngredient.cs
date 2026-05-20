using UnityEngine;
using UnityEngine.SceneManagement;

public class FridgeIngredient : MonoBehaviour
{
    public IngredientType ingredientType;
    public GameObject highlight;

    public string kitchenSceneName = "UpDown"; // change if exact scene name differs

    private bool hasOrderManager;

    private void Start()
    {
        StartCoroutine(WaitForOrderManagerAndUpdate());
    }

    private System.Collections.IEnumerator WaitForOrderManagerAndUpdate()
    {
        while (OrderManager.Instance == null)
        {
            yield return null;
        }

        hasOrderManager = true;
        UpdateHighlight();
    }

    private void Update()
    {
        if (!hasOrderManager && OrderManager.Instance != null)
        {
            hasOrderManager = true;
        }

        if (hasOrderManager)
        {
            UpdateHighlight();
        }
    }

    private void OnMouseDown()
    {
        PickIngredient();
    }

    public void PickIngredient()
    {
        if (OrderManager.Instance == null)
        {
            Debug.LogError("FridgeIngredient: OrderManager instance is missing.");
            return;
        }

        bool needed = OrderManager.Instance.ActiveReceiptNeedsIngredient(ingredientType);
        Debug.Log("FridgeIngredient: " + ingredientType + " needed? " + needed);

        if (!needed)
        {
            Debug.Log("Wrong ingredient: " + ingredientType);
            UpdateHighlight();
            return;
        }

        Debug.Log("Picked correct ingredient: " + ingredientType);
        OrderManager.Instance.PickIngredientForActiveReceipt(ingredientType);

        if (!Application.CanStreamedLevelBeLoaded(kitchenSceneName))
        {
            Debug.LogError("Cannot load scene: " + kitchenSceneName + ". Check that it is added to Build Settings and the name is correct.");
            return;
        }

        Debug.Log("Loading kitchen scene: " + kitchenSceneName);
        SceneManager.LoadScene(kitchenSceneName);
    }

    private void UpdateHighlight()
    {
        Debug.Log("Checking highlight for: " + ingredientType);

        if (OrderManager.Instance == null)
        {
            Debug.LogError("NO ORDER MANAGER FOUND");
            return;
        }

        var activeReceipt = OrderManager.Instance.ActiveReceipt;
        Debug.Log("Active receipt type = " + (activeReceipt != null ? activeReceipt.orderType.ToString() : "null"));

        bool neededNow = OrderManager.Instance.ActiveReceiptNeedsIngredient(ingredientType);
        bool neededLater = OrderManager.Instance.ActiveReceiptUsesIngredient(ingredientType);
        bool needed = neededNow || neededLater;

        Debug.Log(ingredientType + " needed now? " + neededNow + ", needed later? " + neededLater);

        if (highlight == null)
        {
            Debug.LogError("NO HIGHLIGHT ASSIGNED");
            return;
        }

        highlight.SetActive(needed);
    }
}