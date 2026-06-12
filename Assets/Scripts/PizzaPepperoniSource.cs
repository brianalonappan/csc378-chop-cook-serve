using UnityEngine;
using UnityEngine.SceneManagement;

public class PizzaPepperoniSource : MonoBehaviour
{
    public PizzaPepperoniDropBox dropBox;
    public GameObject pepperoniSlicePrefab;
    public Transform pizzaReference;

    public int maxSlices = 8;
    public int slicesNeededToFinish = 8;
    public string sceneToLoad = "UpDown";

    private Vector3 startPosition;
    private Vector3 dragOffset;
    private bool isDragging;
    private int placedSlices = 0;
    private Collider2D sourceCollider;
    private AudioSource audioSource;

    private void Awake()
    {
        sourceCollider = GetComponent<Collider2D>();
        audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        if (dropBox == null)
            dropBox = FindAnyObjectByType<PizzaPepperoniDropBox>();

        if (pizzaReference == null)
            pizzaReference = dropBox != null ? dropBox.transform : null;

        EnsureCollider();
        startPosition = transform.position;

        placedSlices = 0;
        PizzaState.pepperoniCount = 0;
        PizzaState.pepperoniLocalPositions.Clear();
        PizzaState.pepperoniLocalRotations.Clear();
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

        if (audioSource != null && audioSource.clip != null)
        {
            audioSource.PlayOneShot(audioSource.clip);
            Debug.Log("Played pepperoni sound");
        }

        placedSlices++;
        PizzaState.pepperoniCount = placedSlices;

        if (pizzaReference != null)
        {
            Vector3 localPosition = pizzaReference.InverseTransformPoint(spawnPosition);
            PizzaState.pepperoniLocalPositions.Add(localPosition);
            PizzaState.pepperoniLocalRotations.Add(Quaternion.Inverse(pizzaReference.rotation) * slice.transform.rotation);
            Debug.Log("Saved pepperoni local position: " + localPosition + " | count: " + PizzaState.pepperoniCount);
        }
        else
        {
            Debug.LogWarning("Pizza reference missing, could not save local pepperoni position.");
        }

        if (placedSlices >= Mathf.Min(slicesNeededToFinish, maxSlices))
        {
            if (OrderManager.Instance != null)
                OrderManager.Instance.CompletePizzaPrepForActiveReceipt();

            Debug.Log("Loading scene: " + sceneToLoad);

            if (OrderManager.Instance != null)
                OrderManager.Instance.PrepareForSceneLoad();

            SceneManager.LoadScene(sceneToLoad);
        }
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