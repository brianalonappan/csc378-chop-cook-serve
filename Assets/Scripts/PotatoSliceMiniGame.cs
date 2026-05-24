using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PotatoSliceMinigame : MonoBehaviour
{
    public GameObject[] cutMarks;
    public GameObject rawFries;
    public SpriteRenderer potatoRenderer;
    public string returnSceneName = "UpDown";

    private int sliceCount = 0;
    private bool finished = false;

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

        if (OrderManager.Instance != null && OrderManager.Instance.ActiveReceipt != null)
        {
            OrderManager.Instance.ActiveReceipt.CompletePotatoChopping();
        }

        SceneManager.LoadScene(returnSceneName);
    }
}