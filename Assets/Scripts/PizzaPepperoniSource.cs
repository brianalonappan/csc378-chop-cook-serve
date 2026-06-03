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
    private Collider2D sourceCollider;

    private void Awake()
    {
        sourceCollider = GetComponent<Collider2D>();
    }

    private void Start()
    {
        if (dropBox == null)
            dropBox = FindAnyObjectByType<PizzaPepperoniDropBox>();

        EnsureCollider();
        startPosition = transform.position;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
            TryStartDrag();

        if (Input.GetMouseButton(0) && isDragging)
            DragToPointer();

        if (Input.GetMouseButtonUp(0) && isDragging)
            FinishDrag();
    }

    private void TryStartDrag()
    {
        Vector3 mouseWorldPosition = GetMouseWorldPosition();
        if (sourceCollider == null || !sourceCollider.OverlapPoint(mouseWorldPosition))
            return;

        startPosition = transform.position;
        dragOffset = transform.position - mouseWorldPosition;
        isDragging = true;
    }

    private void DragToPointer()
    {
        transform.position = GetMouseWorldPosition() + dragOffset;
    }

    private void FinishDrag()
    {
        bool droppedOnPizza = dropBox != null && dropBox.ContainsWorldPoint(transform.position);

        if (droppedOnPizza && placedSlices < maxSlices)
        {
            PlacePepperoniSlice();
        }

        transform.position = startPosition;
        isDragging = false;
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
        if (sourceCollider != null)
            return;

        BoxCollider2D boxCollider = gameObject.AddComponent<BoxCollider2D>();
        sourceCollider = boxCollider;

        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null && spriteRenderer.sprite != null)
        {
            boxCollider.size = spriteRenderer.sprite.bounds.size;
            boxCollider.offset = spriteRenderer.sprite.bounds.center;
        }
    }
}
