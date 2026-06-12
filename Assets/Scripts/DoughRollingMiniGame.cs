using UnityEngine;

public class DoughRollingMiniGame : MonoBehaviour
{
    public Transform doughTransform;
    public Vector3 startScale = new Vector3(1.5f, 1.5f, 1f);
    public Vector3 endScale = new Vector3(2.6f, 1.9f, 1f);

    public float progressNeeded = 12f;
    public float horizontalWeight = 1.0f;
    public float verticalWeight = 0.35f;

    private bool isDragging = false;
    private bool finishedStretching = false;
    private Vector3 lastMouseWorldPos;
    private float stretchProgress = 0f;
    private AudioSource audioSource;

    private void Start()
    {
        if (doughTransform == null)
        {
            doughTransform = transform;
        }

        doughTransform.localScale = startScale;
        audioSource = GetComponent<AudioSource>();
    }

    private void OnMouseDown()
    {
        if (finishedStretching)
            return;

        isDragging = true;
        lastMouseWorldPos = GetMouseWorldPosition();

        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.Play();
        }
    }

    private void OnMouseUp()
    {
        isDragging = false;
        if (audioSource != null)
        {
            audioSource.Stop();
        }
    }

    private void Update()
    {
        if (!isDragging || finishedStretching)
            return;

        Vector3 currentMouseWorldPos = GetMouseWorldPosition();
        Vector3 delta = currentMouseWorldPos - lastMouseWorldPos;

        float weightedMovement =
            Mathf.Abs(delta.x) * horizontalWeight +
            Mathf.Abs(delta.y) * verticalWeight;

        stretchProgress += weightedMovement;
        stretchProgress = Mathf.Clamp(stretchProgress, 0f, progressNeeded);

        float t = stretchProgress / progressNeeded;
        doughTransform.localScale = Vector3.Lerp(startScale, endScale, t);

        lastMouseWorldPos = currentMouseWorldPos;

        if (stretchProgress >= progressNeeded)
        {
            finishedStretching = true;
            isDragging = false;
            Debug.Log("Dough stretching complete");
        }
    }

    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mouseScreenPos = Input.mousePosition;
        mouseScreenPos.z = Mathf.Abs(Camera.main.transform.position.z);
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);
        mouseWorldPos.z = 0f;
        return mouseWorldPos;
    }
}