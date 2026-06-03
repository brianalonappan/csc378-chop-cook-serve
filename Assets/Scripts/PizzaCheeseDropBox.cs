using UnityEngine;

public class PizzaCheeseDropBox : MonoBehaviour
{
    public Collider2D boxCollider;

    private void Awake()
    {
        if (boxCollider == null)
            boxCollider = GetComponent<Collider2D>();
    }

    public bool ContainsWorldPoint(Vector3 worldPoint)
    {
        if (boxCollider == null)
        {
            Debug.LogError("PizzaCheeseDropBox needs a Collider2D.");
            return false;
        }

        return boxCollider.OverlapPoint(worldPoint);
    }
}