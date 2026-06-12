using UnityEngine;

public class IngredientClickTarget : MonoBehaviour
{
    [SerializeField] private BowlController bowlController;
    [SerializeField] private string ingredientName = "salt";
    [SerializeField] private Camera inputCamera;
    [SerializeField] private AudioSource bowlAudioSource;

    private Vector3 startPosition;
    private Vector3 dragOffset;
    private bool isDragging;

    private void Awake()
    {
        if (bowlController == null)
            bowlController = FindAnyObjectByType<BowlController>();

        if (inputCamera == null)
            inputCamera = Camera.main;

        if (bowlAudioSource == null && bowlController != null)
            bowlAudioSource = bowlController.GetComponent<AudioSource>();

        startPosition = transform.position;
    }

    private void OnMouseDown()
    {
        isDragging = true;
        dragOffset = transform.position - GetPointerWorldPosition();
    }

    private void OnMouseDrag()
    {
        if (!isDragging)
            return;

        Vector3 position = GetPointerWorldPosition() + dragOffset;
        transform.position = new Vector3(position.x, position.y, transform.position.z);
    }

    private void OnMouseUp()
    {
        if (!isDragging)
            return;

        isDragging = false;

        if (bowlController != null &&
            bowlController.TryAddIngredient(ingredientName, transform.position))
        {
            if (bowlAudioSource != null)
            {
                bowlAudioSource.Stop();
                bowlAudioSource.Play();
            }

            gameObject.SetActive(false);
            return;
        }

        transform.position = startPosition;
    }

    public void AddIngredient()
    {
        if (bowlController != null)
            bowlController.OnIngredientAdded(ingredientName);
    }

    private Vector3 GetPointerWorldPosition()
    {
        if (inputCamera == null)
            inputCamera = Camera.main;

        Vector3 pointerPosition = inputCamera.ScreenToWorldPoint(Input.mousePosition);
        pointerPosition.z = transform.position.z;
        return pointerPosition;
    }
}