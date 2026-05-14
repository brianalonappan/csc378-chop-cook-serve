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

    private bool playerInRange = false;
    private HeldFoodVisuals heldFoodVisuals;

    private void Start()
    {
        heldFoodVisuals = FindFirstObjectByType<HeldFoodVisuals>();
    }

    private void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            if (heldFoodVisuals == null)
                return;

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
                heldFoodVisuals.ShowFries();
            }

            gameObject.SetActive(false);
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
}