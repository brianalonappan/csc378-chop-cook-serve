using UnityEngine;

public class InteractableScript : MonoBehaviour
{
    public string stationName;
    public bool hasBeenInteractedWith = false;

    public void Interact()
    {
        hasBeenInteractedWith = true;
    }
}