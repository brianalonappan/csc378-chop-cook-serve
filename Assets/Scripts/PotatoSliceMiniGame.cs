using UnityEngine;
using UnityEngine.SceneManagement;

public class PotatoSliceMinigame : MonoBehaviour
{
    public GameObject[] cutMarks;
    public string returnSceneName = "UpDown";

    private int sliceCount = 0;

    private void Start()
    {
        for (int i = 0; i < cutMarks.Length; i++)
        {
            if (cutMarks[i] != null)
            {
                cutMarks[i].SetActive(false);
            }
        }
    }

    private void OnMouseDown()
    {
        SlicePotato();
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
            FinishSlicing();
        }
    }

    private void FinishSlicing()
    {
        Debug.Log("Potato slicing complete.");
        SceneManager.LoadScene(returnSceneName);
    }
}