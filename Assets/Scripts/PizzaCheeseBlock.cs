using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class PizzaCheeseBlock : MonoBehaviour
{
    public PizzaCheeseDropBox dropBox;
    public GameObject cheeseOnDough;

    public float sprinkleNeeded = 4f;
    public string sceneToLoad = "CheeseOvenScene";

    private Vector3 startPosition;
    private Vector3 dragOffset;
    private bool isDragging;
    private bool isSprinkling;
    private float sprinkleProgress;
    private Vector3 lastMouseWorldPosition;
    private PizzaSauceBottle sauceBottle;

    private void Start()
    {
        startPosition = transform.position;

        if (dropBox == null)
            dropBox = FindAnyObjectByType<PizzaCheeseDropBox>();

        sauceBottle = FindAnyObjectByType<PizzaSauceBottle>();

        if (cheeseOnDough != null)
            cheeseOnDough.SetActive(false);

        EnsureCollider();
    }

    private void OnMouseDown()
    {
        if (isSprinkling || !CanPlaceCheese())
            return;

        startPosition = transform.position;
        isDragging = false;

        Vector3 mouseWorldPosition = GetMouseWorldPosition();
        dragOffset = transform.position - mouseWorldPosition;
        lastMouseWorldPosition = mouseWorldPosition;
    }

    private void OnMouseDrag()
    {
        if (isSprinkling || !CanPlaceCheese())
            return;

        isDragging = true;
        transform.position = GetMouseWorldPosition() + dragOffset;
    }

    private void OnMouseUp()
    {
        if (isSprinkling)
            return;

        if (isDragging)
        {
            bool droppedOnPizza = dropBox != null && dropBox.ContainsWorldPoint(transform.position);

            if (droppedOnPizza && CanPlaceCheese())
            {
                StartCoroutine(SprinkleSequence());
                return;
            }

            transform.position = startPosition;
        }
    }

    private bool CanPlaceCheese()
    {
        return sauceBottle == null || sauceBottle.SauceAdded;
    }

    private IEnumerator SprinkleSequence()
    {
        isSprinkling = true;
        sprinkleProgress = 0f;
        lastMouseWorldPosition = GetMouseWorldPosition();

        while (sprinkleProgress < sprinkleNeeded)
        {
            Vector3 currentMouseWorldPosition = GetMouseWorldPosition();
            sprinkleProgress += Vector3.Distance(currentMouseWorldPosition, lastMouseWorldPosition);
            lastMouseWorldPosition = currentMouseWorldPosition;

            transform.position = currentMouseWorldPosition;

            Debug.Log("Cheese progress: " + sprinkleProgress + " / " + sprinkleNeeded);
            yield return null;
        }

        if (cheeseOnDough != null)
            cheeseOnDough.SetActive(true);

        transform.position = startPosition;
        isSprinkling = false;

        yield return new WaitForSeconds(2f);

        Debug.Log("Loading scene: " + sceneToLoad);
        SceneManager.LoadScene(sceneToLoad);
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