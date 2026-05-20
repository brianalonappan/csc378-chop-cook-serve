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

    private bool playerInRange = false;
    private HeldFoodVisuals heldFoodVisuals;
    private AudioSource audioSource;

    private void Start()
    {
        heldFoodVisuals = FindAnyObjectByType<HeldFoodVisuals>();
        audioSource = GetComponent<AudioSource>();
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

            if (audioSource != null && pickupSound != null)
            {
                audioSource.PlayOneShot(pickupSound);
            }

            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            Collider2D collider2d = GetComponent<Collider2D>();

            if (spriteRenderer != null)
                spriteRenderer.enabled = false;

            if (collider2d != null)
                collider2d.enabled = false;
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