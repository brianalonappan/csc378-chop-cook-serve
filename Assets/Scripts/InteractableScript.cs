using UnityEngine;
using UnityEngine.SceneManagement;

public class InteractableScript : MonoBehaviour
{
    public string stationName;
    public StationType stationType;

    public bool hasBeenInteractedWith = false;

    // Scene only used for fridge
    public string fridgeSceneName = "Fridge Detailed";

    public void Interact()
    {
        hasBeenInteractedWith = true;

        Debug.Log(stationName + " interacted with.");

        // ONLY fridge loads fridge scene
        if (stationType == StationType.Fridge)
        {
            if (OrderManager.Instance != null)
                OrderManager.Instance.PrepareForSceneLoad();

            SceneManager.LoadScene(fridgeSceneName);
            return;
        }

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
