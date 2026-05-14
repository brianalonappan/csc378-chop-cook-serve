using UnityEngine;

public class InteractableScript : MonoBehaviour
{
    public string stationName;
    public StationType stationType;

    public bool hasBeenInteractedWith = false;

    public void Interact()
    {
        hasBeenInteractedWith = true;

        Debug.Log(stationName + " interacted with.");

        if (OrderManager.Instance != null)
        {
            OrderManager.Instance.TryUseStation(stationType);
        }
        else
        {
            Debug.LogWarning("No OrderManager found in scene.");
        }
    }
}