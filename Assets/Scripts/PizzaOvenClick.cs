using UnityEngine;

public class PizzaOvenClick : MonoBehaviour
{
    public Transform ovenDropPoint;

    private bool hasMoved = false;

    private void OnMouseDown()
    {
        if (hasMoved)
            return;

        if (ovenDropPoint != null)
        {
            transform.position = ovenDropPoint.position;
            hasMoved = true;
        }
    }
}