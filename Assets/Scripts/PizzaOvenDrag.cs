using UnityEngine;
using System.Collections;

public class PizzaOvenDrag : MonoBehaviour
{
    public Collider2D ovenDropBox;
    public Transform ovenDropPoint;

    public GameObject uncookedPizza;
    public GameObject cookedPizza;

    public float bakeTime = 1.5f;
    public string sceneToLoad = "UpDown";
    public bool returnToSceneAfterBake = true;

    private Vector3 startPosition;
    private Vector3 dragOffset;
    private bool isDragging;
    private bool isBaking;
    private AudioSource audioSource;

    private void Start()
    {
        startPosition = transform.position;
        EnsureCollider();
        audioSource = GetComponent<AudioSource>();

        if (uncookedPizza != null)
            uncookedPizza.SetActive(true);

        if (cookedPizza != null)
            cookedPizza.SetActive(false);

        if (audioSource != null)
            Debug.Log("Found oven audio source");
        else
            Debug.Log("No oven audio source found");
    }

    private void OnMouseDown()
    {
        if (isBaking) return;

        startPosition = transform.position;
        isDragging = false;

        Vector3 mouseWorldPosition = GetMouseWorldPosition();
        dragOffset = transform.position - mouseWorldPosition;
    }

    private void OnMouseDrag()
    {
        if (isBaking) return;

        isDragging = true;
        transform.position = GetMouseWorldPosition() + dragOffset;
    }

    private void OnMouseUp()
    {
        if (isBaking) return;

        if (isDragging)
        {
            bool droppedInOven = ovenDropBox != null && ovenDropBox.OverlapPoint(transform.position);

            if (droppedInOven)
            {
                StartCoroutine(BakeSequence());
                return;
            }

            transform.position = startPosition;
        }
    }

    private IEnumerator BakeSequence()
    {
        isBaking = true;
        isDragging = false;

        if (ovenDropPoint != null)
            transform.position = ovenDropPoint.position;

        if (audioSource != null)
        {
            Debug.Log("Starting oven fire sound");
            audioSource.Stop();
            audioSource.loop = true;
            audioSource.Play();
        }

        yield return new WaitForSeconds(bakeTime);

        if (audioSource != null)
        {
            Debug.Log("Stopping oven fire sound");
            audioSource.Stop();
        }

        if (uncookedPizza != null)
            uncookedPizza.SetActive(false);

        if (cookedPizza != null)
            cookedPizza.SetActive(true);

        if (OrderManager.Instance != null)
            OrderManager.Instance.TryUseStation(StationType.Oven);

        isBaking = false;

        if (returnToSceneAfterBake && !string.IsNullOrEmpty(sceneToLoad))
        {
            if (OrderManager.Instance != null)
                OrderManager.Instance.PrepareForSceneLoad();

            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneToLoad);
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
        if (GetComponent<Collider2D>() != null)
            return;

        BoxCollider2D boxCollider = gameObject.AddComponent<BoxCollider2D>();
        boxCollider.size = new Vector2(2.5f, 1.2f);
        boxCollider.offset = Vector2.zero;
    }
}