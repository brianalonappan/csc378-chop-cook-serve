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
    public string potatoMixSceneName = "MixPotato";
    public string fryingSceneName = "FryingScene";
    public string cheesePizzaSceneName = "CheeseScene";
    public string pepperoniPizzaSceneName = "PepperoniScene";
    public string cheeseOvenSceneName = "CheeseOvenScene";
    public string pepperoniOvenSceneName = "PepperoniOvenScene";

    public void Interact(Vector3 playerPosition)
    {
        hasBeenInteractedWith = true;

        Debug.Log(stationName + " interacted with.");

        if (stationType == StationType.Fridge)
        {
            if (OrderManager.Instance != null)
            {
                OrderManager.Instance.SavePlayerReturnPosition(playerPosition);
                OrderManager.Instance.PrepareForSceneLoad();
            }

            SceneManager.LoadScene(fridgeSceneName);
            return;
        }

        if (stationType == StationType.ChoppingBlock &&
            OrderManager.Instance != null &&
            OrderManager.Instance.ActiveReceiptNeedsPotatoChopping())
        {
            OrderManager.Instance.SavePlayerReturnPosition(playerPosition);
            OrderManager.Instance.PrepareForSceneLoad();
            SceneManager.LoadScene(potatoCuttingSceneName);
            return;
        }

        if (stationType == StationType.ChoppingBlock &&
            OrderManager.Instance != null &&
            OrderManager.Instance.ActiveReceiptNeedsPizzaPrep())
        {
            OrderManager.Instance.SavePlayerReturnPosition(playerPosition);
            OrderManager.Instance.PrepareForSceneLoad();

            string pizzaSceneName =
                OrderManager.Instance.ActiveOrderType == OrderType.PepperoniPizza
                    ? pepperoniPizzaSceneName
                    : cheesePizzaSceneName;

            SceneManager.LoadScene(pizzaSceneName);
            return;
        }

        if (stationType == StationType.ToppingsTable &&
            OrderManager.Instance != null &&
            OrderManager.Instance.ActiveReceiptNeedsPotatoMixing())
        {
            OrderManager.Instance.SavePlayerReturnPosition(playerPosition);
            OrderManager.Instance.PrepareForSceneLoad();
            SceneManager.LoadScene(potatoMixSceneName);
            return;
        }

        if (stationType == StationType.Stove &&
            OrderManager.Instance != null &&
            OrderManager.Instance.ActiveReceiptNeedsFriesFrying())
        {
            OrderManager.Instance.SavePlayerReturnPosition(playerPosition);
            OrderManager.Instance.PrepareForSceneLoad();
            SceneManager.LoadScene(fryingSceneName);
            return;
        }

        if (stationType == StationType.Oven &&
            OrderManager.Instance != null &&
            OrderManager.Instance.ActiveReceiptNeedsPizzaBaking())
        {
            OrderManager.Instance.SavePlayerReturnPosition(playerPosition);
            OrderManager.Instance.PrepareForSceneLoad();

            string ovenSceneName =
                OrderManager.Instance.ActiveOrderType == OrderType.PepperoniPizza
                    ? pepperoniOvenSceneName
                    : cheeseOvenSceneName;

            SceneManager.LoadScene(ovenSceneName);
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
