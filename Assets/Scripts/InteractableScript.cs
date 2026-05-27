using UnityEngine;
using UnityEngine.SceneManagement;

public class InteractableScript : MonoBehaviour
{
    public string stationName;
    public StationType stationType;

    public bool hasBeenInteractedWith = false;

    // Scene names used by stations that open focused interaction views.
    public string fridgeSceneName = "Fridge Detailed";
    public string potatoCuttingSceneName = "CuttingBoardPotato";

    public void Interact()
    {
        hasBeenInteractedWith = true;

        Debug.Log(stationName + " interacted with.");

        if (stationType == StationType.Fridge)
        {
            if (OrderManager.Instance != null)
                OrderManager.Instance.PrepareForSceneLoad();

            SceneManager.LoadScene(fridgeSceneName);
            return;
        }

        if (stationType == StationType.ChoppingBlock &&
            OrderManager.Instance != null &&
            OrderManager.Instance.ActiveReceiptNeedsPotatoChopping())
        {
            OrderManager.Instance.PrepareForSceneLoad();
            SceneManager.LoadScene(potatoCuttingSceneName);
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
