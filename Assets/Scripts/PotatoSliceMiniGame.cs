using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PotatoSliceMinigame : MonoBehaviour
{
    public GameObject[] cutMarks;
    public GameObject rawFries;
    public SpriteRenderer potatoRenderer;
    public OrderState orderState;
    public string returnSceneName = "UpDown";

    private int sliceCount = 0;
    private bool finished = false;
    private AudioSource audioSource;

    private void Start()
    {
        for (int i = 0; i < cutMarks.Length; i++)
        {
            if (cutMarks[i] != null)
            {
                cutMarks[i].SetActive(false);
            }
        }

        if (rawFries != null)
        {
            rawFries.SetActive(false);
        }

        if (potatoRenderer == null)
        {
            potatoRenderer = GetComponent<SpriteRenderer>();
        }

        audioSource = GetComponent<AudioSource>();
    }

    private void OnMouseDown()
    {
        if (!finished)
        {
            SlicePotato();
        }
    }

    private void SlicePotato()
    {
        if (sliceCount < cutMarks.Length)
        {
            if (cutMarks[sliceCount] != null)
            {
                cutMarks[sliceCount].SetActive(true);
            }

            if (audioSource != null)
            {
                audioSource.Stop();
                audioSource.Play();
                Debug.Log("Played potato chop sound");
            }

            sliceCount++;
            Debug.Log("Potato sliced: " + sliceCount);
        }

        if (sliceCount >= cutMarks.Length)
        {
            finished = true;
            StartCoroutine(FinishSlicing());
        }
    }

    private IEnumerator FinishSlicing()
    {
        if (potatoRenderer != null)
        {
            potatoRenderer.enabled = false;
        }

        for (int i = 0; i < cutMarks.Length; i++)
        {
            if (cutMarks[i] != null)
            {
                cutMarks[i].SetActive(false);
            }
        }

        if (rawFries != null)
        {
            rawFries.SetActive(true);
        }

        yield return new WaitForSeconds(2.5f);

        CompletePotatoChoppingAndReturnToKitchen();
    }

    private void CompletePotatoChoppingAndReturnToKitchen()
    {
        if (OrderManager.Instance == null)
        {
            Debug.LogError("PotatoSliceMinigame: OrderManager instance is missing.");
            return;
        }

        OrderState state = GetOrderState();
        bool readyForPotatoChopping = state?.hasActiveOrder == true &&
            state.activeOrderType == OrderType.FrenchFries &&
            state.grabbedIngredients &&
            !state.choppedOrStretched;

        if (!readyForPotatoChopping && !OrderManager.Instance.ActiveReceiptNeedsPotatoChopping())
        {
            Debug.Log("Potato chopping is not ready for the active receipt.");
            return;
        }

        if (!OrderManager.Instance.CompletePotatoChoppingForActiveReceipt())
        {
            Debug.Log("Potato was cut, but the active receipt did not accept the chopping step.");
            return;
        }

        string sceneToLoad = !string.IsNullOrEmpty(OrderManager.Instance.kitchenSceneName)
            ? OrderManager.Instance.kitchenSceneName
            : returnSceneName;

        if (!Application.CanStreamedLevelBeLoaded(sceneToLoad))
        {
            Debug.LogError("Cannot load scene: " + sceneToLoad + ". Check that it is added to Build Settings and the name is correct.");
            return;
        }

        Debug.Log("Loading kitchen scene: " + sceneToLoad);
        OrderManager.Instance.PrepareForSceneLoad();
        SceneManager.LoadScene(sceneToLoad);
    }

    private OrderState GetOrderState()
    {
        if (orderState != null)
            return orderState;

        return OrderManager.Instance != null ? OrderManager.Instance.State : null;
    }
}