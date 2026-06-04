using UnityEngine;

public class CounterFoodPickup : MonoBehaviour
{
    public enum FoodType
    {
        PepperoniPizza,
        CheesePizza,
        Fries
    }

    public FoodType foodType;
    public AudioClip pickupSound;
    public OrderState orderState;
    public Color normalFoodColor = Color.white;
    public Color burnedFriesColor = new Color(0.18f, 0.12f, 0.07f, 1f);

    private bool playerInRange = false;
    private HeldFoodVisuals heldFoodVisuals;
    private AudioSource audioSource;
    private SpriteRenderer spriteRenderer;
    private Collider2D pickupCollider;
    private bool hasBeenPickedUp = false;

    private void Start()
    {
        heldFoodVisuals = FindAnyObjectByType<HeldFoodVisuals>();
        audioSource = GetComponent<AudioSource>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        pickupCollider = GetComponent<Collider2D>();

        UpdateCounterFoodVisibility();
    }

    private void Update()
    {
        UpdateCounterFoodVisibility();

        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            if (heldFoodVisuals == null)
                return;

            if (heldFoodVisuals.IsHoldingFood)
            {
                Debug.Log("Already holding food.");
                return;
            }

            if (foodType == FoodType.PepperoniPizza)
            {
                heldFoodVisuals.ShowPepperoniPizza();
            }
            else if (foodType == FoodType.CheesePizza)
            {
                heldFoodVisuals.ShowCheesePizza();
            }
            else if (foodType == FoodType.Fries)
            {
                bool burned = IsFoodBurned();
                heldFoodVisuals.ShowFries(burned);
            }

            if (audioSource != null && pickupSound != null)
            {
                audioSource.PlayOneShot(pickupSound);
            }

            hasBeenPickedUp = true;

            if (spriteRenderer != null)
                spriteRenderer.enabled = false;

            if (pickupCollider != null)
                pickupCollider.enabled = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    private void ApplyFoodStateVisual()
    {
        if (spriteRenderer == null)
            return;

        if (foodType != FoodType.Fries)
        {
            spriteRenderer.color = normalFoodColor;
            return;
        }

        spriteRenderer.color = IsFoodBurned() ? burnedFriesColor : normalFoodColor;
    }

    private void UpdateCounterFoodVisibility()
    {
        bool shouldShow = ShouldShowForActiveOrder();

        if (!shouldShow)
            hasBeenPickedUp = false;

        bool visible = shouldShow && !hasBeenPickedUp;

        if (!visible)
            playerInRange = false;

        if (spriteRenderer != null)
            spriteRenderer.enabled = visible;

        if (pickupCollider != null)
            pickupCollider.enabled = visible;

        if (visible)
            ApplyFoodStateVisual();
    }

    private bool ShouldShowForActiveOrder()
    {
        OrderState state = GetOrderState();

        if (state == null || !state.hasActiveOrder || state.served || state.finishedOrder)
            return false;

        switch (foodType)
        {
            case FoodType.Fries:
                return state.activeOrderType == OrderType.FrenchFries &&
                    state.cookedOrBaked;

            case FoodType.CheesePizza:
                return state.activeOrderType == OrderType.CheesePizza &&
                    state.cutPizza;

            case FoodType.PepperoniPizza:
                return state.activeOrderType == OrderType.PepperoniPizza &&
                    state.cutPizza;
        }

        return false;
    }

    private OrderState GetOrderState()
    {
        if (orderState != null)
            return orderState;

        return OrderManager.Instance != null ? OrderManager.Instance.State : null;
    }

    private bool IsFoodBurned()
    {
        OrderState state = GetOrderState();
        return state != null && state.foodBurned;
    }
}
