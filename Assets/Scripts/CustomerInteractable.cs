using UnityEngine;

public class CustomerInteractable : MonoBehaviour
{
    private CustomerWalker customerWalker;
    private bool playerInRange;

    private void Awake()
    {
        customerWalker = GetComponent<CustomerWalker>();
    }

    private void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
            Interact();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInRange = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInRange = false;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInRange = true;
    }

    public void Interact()
    {
        if (OrderManager.Instance == null)
        {
            Debug.LogWarning("No OrderManager found in scene.");
            return;
        }

        if (customerWalker != null && customerWalker.reachedRegister)
        {
            OrderManager.Instance.TryUseStation(StationType.Cashier);
            return;
        }

        if (customerWalker != null && customerWalker.waitingForFood)
        {
            OrderManager.Instance.TryUseStation(StationType.DropOff);
            return;
        }

        Debug.Log("Customer is not ready to interact yet.");
    }
}
