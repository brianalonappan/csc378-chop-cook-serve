using UnityEngine;
using System.Collections;

public class FryFriesMiniGame : MonoBehaviour
{
    public GameObject rawFries;
    public GameObject cookedFries;
    public AudioSource fryingSound;

    private bool startedFrying = false;

    private void Start()
    {
        if (rawFries != null)
            rawFries.SetActive(true);

        if (cookedFries != null)
            cookedFries.SetActive(false);
    }

    private void OnMouseDown()
    {
        if (!startedFrying)
        {
            startedFrying = true;
            StartCoroutine(FrySequence());
        }
    }

    private IEnumerator FrySequence()
    {
        if (fryingSound != null)
        {
            fryingSound.Play();
        }

        yield return new WaitForSeconds(2.5f);

        if (fryingSound != null)
        {
            fryingSound.Stop();
        }

        if (rawFries != null)
            rawFries.SetActive(false);

        if (cookedFries != null)
            cookedFries.SetActive(true);
    }
}