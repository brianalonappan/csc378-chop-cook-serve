using UnityEngine;

public class PizzaPepperoniSource : MonoBehaviour
{
    public PizzaPepperoniDropBox dropBox;
    public GameObject pepperoniSlicePrefab;

    public int maxSlices = 5;
    public Vector2 placementAreaSize = new Vector2(1.8f, 1.0f);

    private Vector3 startPosition;
    private Vector3 dragOffset;
    private bool isDragging;
    private int placedSlices = 0;

    private void Start()
    {
        startPosition = transform.position;

        if (dropBox == null)
            dropBox = FindAnyObjectByType<PizzaPepperoniDropBox>();

        EnsureCollider();
    }

    private void OnMouseDown()
    {
        Vector3 mouseWorldPosition = GetMouseWorldPosition();
        dragOffset = transform.position - mouseWorldPosition;
        isDragging = false;
    }

    private void OnMouseDrag()
    {
        isDragging = true;
        transform.position = GetMouseWorldPosition() + dragOffset;
    }

    private void OnMouseUp()
    {
        if (isDragging)
        {
            bool droppedOnPizza = dropBox != null && dropBox.ContainsWorldPoint(transform.position);

            if (droppedOnPizza && placedSlices < maxSlices)
            {
                PlacePepperoniSlice();
            }

            transform.position = startPosition;
        }
    }

    private void PlacePepperoniSlice()
    {
        if (pepperoniSlicePrefab == null)
            return;

        Vector3 spawnPosition = transform.position;
        spawnPosition.z = 0f;

        GameObject slice = Instantiate(pepperoniSlicePrefab, spawnPosition, Quaternion.identity);

        float randomRotation = Random.Range(-25f, 25f);
        slice.transform.rotation = Quaternion.Euler(0f, 0f, randomRotation);

        placedSlices++;
        Debug.Log("Placed pepperoni slice: " + placedSlices);
    }

    private Vector3 GetMouseWorldPosition()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
            return transform.position;

        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = Mathf.Abs(mainCamera.transform.position.z - transform.position.z);
        return mainCamera.ScreenToWorldPoint(mousePosition);
    }

    private void EnsureCollider()
    {
        if (GetComponent<Collider2D>() != null)
            return;

        BoxCollider2D boxCollider = gameObject.AddComponent<BoxCollider2D>();
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null && spriteRenderer.sprite != null)
        {
            boxCollider.size = spriteRenderer.sprite.bounds.size;
            boxCollider.offset = spriteRenderer.sprite.bounds.center;
        }
    }
}