using UnityEngine;
using System.Collections;

public class PizzaSauceBottle : MonoBehaviour
{
    public PizzaSauceDropBox dropBox;
    public Transform pourPoint;
    public GameObject saucePourStream;
    public GameObject sauceBlob;
    public GameObject sauceOnDough;

    private Vector3 startPosition;
    private Vector3 dragOffset;
    private bool isDragging;
    private bool isPouring;

    public bool SauceAdded { get; private set; }

    private void Start()
    {
        startPosition = transform.position;
        SauceAdded = false;

        if (dropBox == null)
            dropBox = FindAnyObjectByType<PizzaSauceDropBox>();

        if (saucePourStream != null)
            saucePourStream.SetActive(false);

        if (sauceOnDough != null)
            sauceOnDough.SetActive(false);

        EnsureCollider();
    }

    private void OnMouseDown()
    {
        if (isPouring)
            return;

        startPosition = transform.position;
        isDragging = false;

        Vector3 mouseWorldPosition = GetMouseWorldPosition();
        dragOffset = transform.position - mouseWorldPosition;
    }

    private void OnMouseDrag()
    {
        if (isPouring)
            return;

        isDragging = true;
        transform.position = GetMouseWorldPosition() + dragOffset;
    }

    private void OnMouseUp()
    {
        if (isPouring)
            return;

        if (isDragging)
        {
            bool droppedOnDough = dropBox != null && dropBox.ContainsWorldPoint(transform.position);

            if (droppedOnDough)
            {
                StartCoroutine(PourSequence());
                return;
            }

            transform.position = startPosition;
        }
    }

    private IEnumerator PourSequence()
    {
        isPouring = true;
        isDragging = false;

        if (pourPoint != null)
            transform.position = pourPoint.position;

        transform.rotation = Quaternion.Euler(0f, 0f, -45f);

        if (saucePourStream != null)
            saucePourStream.SetActive(true);

        yield return new WaitForSeconds(0.5f);

        if (sauceBlob != null)
            sauceBlob.SetActive(true);

        yield return new WaitForSeconds(0.7f);

        if (saucePourStream != null)
            saucePourStream.SetActive(false);

        if (sauceBlob != null)
            sauceBlob.SetActive(false);

        if (sauceOnDough != null)
            sauceOnDough.SetActive(true);

        SauceAdded = true;

        transform.position = startPosition;
        transform.rotation = Quaternion.identity;

        isPouring = false;
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
