using UnityEngine;
using UnityEngine.SceneManagement;

public class FridgeIngredient : MonoBehaviour
{
    public IngredientType ingredientType;
    public GameObject highlight;
    public OrderState orderState;
    public FridgeIngredientDropBox dropBox;
    public bool submitOnClick = true;
    public bool submitOnDrop = true;
    public bool hideWhenBagged = true;

    public string kitchenSceneName = "UpDown"; // change if exact scene name differs

    private Vector3 startPosition;
    private Vector3 dragOffset;
    private bool isDragging;

    private void Awake()
    {
        if (IsHighlightMarker())
        {
            enabled = false;
            return;
        }

        EnsureCollider();
    }

    private void Start()
    {
        startPosition = transform.position;

        if (dropBox == null)
            dropBox = FindAnyObjectByType<FridgeIngredientDropBox>();

        UpdateHighlight();
    }

    private void OnMouseDown()
    {
        if (IsHighlightMarker())
            return;

        startPosition = transform.position;
        isDragging = false;

        Vector3 mouseWorldPosition = GetMouseWorldPosition();
        dragOffset = transform.position - mouseWorldPosition;
    }

    private void OnMouseDrag()
    {
        if (IsHighlightMarker())
            return;

        isDragging = true;
        transform.position = GetMouseWorldPosition() + dragOffset;
    }

    private void OnMouseUp()
    {
        if (IsHighlightMarker())
            return;

        if (isDragging)
        {
            bool droppedInBox = dropBox != null && dropBox.ContainsWorldPoint(transform.position);

            if (submitOnDrop && droppedInBox && SubmitIngredient())
                return;

            transform.position = startPosition;

            return;
        }

        if (submitOnClick)
            SubmitIngredient();
    }

    public bool SubmitIngredient()
    {
        if (OrderManager.Instance == null)
        {
            Debug.LogError("FridgeIngredient: OrderManager instance is missing.");
            return false;
        }

        OrderState state = GetOrderState();
        bool needed = state?.ActiveOrderNeedsIngredient(ingredientType) ??
            OrderManager.Instance.ActiveReceiptNeedsIngredient(ingredientType);
        Debug.Log("FridgeIngredient: " + ingredientType + " needed? " + needed);

        if (!needed)
        {
            Debug.Log("Wrong ingredient or fridge step is not ready yet: " + ingredientType);
            UpdateHighlight();
            return false;
        }

        Debug.Log("Picked correct ingredient: " + ingredientType);
        if (state != null)
        {
            state.PickIngredient(ingredientType);
            HideIfBagged();
            UpdateAllIngredientHighlights();

            if (!state.HasPickedAllRequiredIngredients())
            {
                Debug.Log("Still need more ingredients for " + state.activeOrderType + ".");
                return true;
            }
        }

        if (!OrderManager.Instance.CompleteFridgeIngredientsForActiveReceipt())
        {
            Debug.Log("Fridge ingredients are picked, but the active receipt did not accept the fridge step.");
            return false;
        }

        string sceneToLoad = !string.IsNullOrEmpty(OrderManager.Instance.kitchenSceneName)
            ? OrderManager.Instance.kitchenSceneName
            : kitchenSceneName;

        if (!Application.CanStreamedLevelBeLoaded(sceneToLoad))
        {
            Debug.LogError("Cannot load scene: " + sceneToLoad + ". Check that it is added to Build Settings and the name is correct.");
            return false;
        }

        Debug.Log("Loading kitchen scene: " + sceneToLoad);
        OrderManager.Instance.PrepareForSceneLoad();
        SceneManager.LoadScene(sceneToLoad);
        return true;
    }

    private void UpdateHighlight()
    {
        OrderState state = GetOrderState();
        bool neededNow = state != null && state.ActiveOrderNeedsIngredient(ingredientType);

        if (highlight == null)
        {
            Debug.LogError("NO HIGHLIGHT ASSIGNED");
            return;
        }

        highlight.SetActive(neededNow);
    }

    private void UpdateAllIngredientHighlights()
    {
        FridgeIngredient[] ingredients = FindObjectsByType<FridgeIngredient>(FindObjectsSortMode.None);
        foreach (FridgeIngredient ingredient in ingredients)
        {
            ingredient.UpdateHighlight();
        }
    }

    private OrderState GetOrderState()
    {
        if (orderState != null)
            return orderState;

        return OrderManager.Instance != null ? OrderManager.Instance.State : null;
    }

    private Vector3 GetMouseWorldPosition()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
            return transform.position;

        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = Mathf.Abs(mainCamera.transform.position.z - transform.position.z);
        return mainCamera.ScreenToWorldPoint(mousePosition);
    }

    private void EnsureCollider()
    {
        if (GetComponent<Collider2D>() != null)
            return;

        BoxCollider2D boxCollider = gameObject.AddComponent<BoxCollider2D>();
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null && spriteRenderer.sprite != null)
        {
            boxCollider.size = spriteRenderer.sprite.bounds.size;
            boxCollider.offset = spriteRenderer.sprite.bounds.center;
        }
    }

    private bool IsHighlightMarker()
    {
        return highlight == gameObject;
    }

    private void HideIfBagged()
    {
        if (!hideWhenBagged)
            return;

        if (highlight != null)
            highlight.SetActive(false);

        gameObject.SetActive(false);
    }
}
