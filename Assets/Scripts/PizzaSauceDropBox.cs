using UnityEngine;

public class PizzaSauceDropBox : MonoBehaviour
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
            Debug.LogError("PizzaSauceDropBox needs a Collider2D.");
            return false;
        }

        return boxCollider.OverlapPoint(worldPoint);
    }
}