using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class FryFriesMiniGame : MonoBehaviour
{
    public GameObject rawFries;
    public GameObject cookedFries;
    public SpriteRenderer friesRenderer;
    public SpriteRenderer rawFriesRenderer;
    public AudioSource fryingSound;
    public OrderState orderState;
    public string returnSceneName = "UpDown";
    public float cookedTime = 3f;
    public float burnTime = 5f;
    public Color rawColor = Color.white;
    public Color cookedColor = new Color(0.95f, 0.75f, 0.28f, 1f);
    public Color burnedColor = new Color(0.18f, 0.12f, 0.07f, 1f);
    public float shuffleRotation = 12f;
    public float rawShakeDuration = 0.25f;
    public float rawShakeAmount = 0.04f;
    public float panShakeDuration = 0.18f;
    public float panShakeAmount = 0.03f;
    public float panShakeRotation = 4f;

    private bool startedFrying = false;
    private bool finished = false;
    private float fryTimer = 0f;
    private float rawShakeTimer = 0f;
    private float panShakeTimer = 0f;
    private Vector3 panStartPosition;
    private Quaternion panStartRotation;
    private Vector3 rawFriesStartPosition;
    private Vector3 cookedFriesStartPosition;

    [Header("Peek Mechanic")]
    public float peekDuration = 0.6f;       // how long the peek lasts
    public float peekCooldown = 1.2f;       // cooldown before you can peek again
    public float peekLiftAmount = 0.08f;    // how far fries lift up visually

    private bool isPeeking = false;
    private float peekTimer = 0f;
    private float peekCooldownTimer = 0f;

    [Header("Drag to Finish")]
    public float dragUpThreshold = 1.5f;   // how far up (in world units) to trigger finish
    public float minCookTimeBeforeDrag = 1f; // prevent instant drag-out

    private bool isDragging = false;
    private Vector3 dragStartWorldPos;
    private Vector3 rawFriesDragStartPos;

    private void Start()
    {
        panStartPosition = transform.position;
        panStartRotation = transform.rotation;

        if (rawFries != null)
        {
            rawFries.SetActive(true);
            rawFriesStartPosition = rawFries.transform.position;

            if (rawFriesRenderer == null)
                rawFriesRenderer = rawFries.GetComponent<SpriteRenderer>();
        }

        if (cookedFries != null)
        {
            cookedFries.SetActive(false);
            cookedFriesStartPosition = cookedFries.transform.position;
        }

        if (friesRenderer == null)
        {
            if (cookedFries != null)
                friesRenderer = cookedFries.GetComponent<SpriteRenderer>();
        }

        if (rawFriesRenderer != null)
            rawFriesRenderer.color = rawColor;
    }

    private void Update()
    {
        if (!startedFrying || finished)
            return;

        fryTimer += Time.deltaTime;
        UpdatePanShake();
        UpdateRawFriesShake();
        UpdateFriesColor();
        UpdatePeek();
        UpdateDrag();

        if (!finished && fryTimer >= burnTime)
        {
            FinishFrying(true);
        }
    }

    private void OnMouseOver()
    {
        // Right-click to peek
        if (Input.GetMouseButtonDown(1) && startedFrying && !finished)
        {
            TryPeek();
        }
    }

    private void TryPeek()
    {
        if (isPeeking || peekCooldownTimer > 0f)
            return;

        isPeeking = true;
        peekTimer = peekDuration;

        // Lift the raw fries slightly to "peek" underneath
        if (rawFries != null)
            rawFries.transform.position = rawFriesStartPosition + new Vector3(0f, peekLiftAmount, 0f);
    }

    private void UpdatePeek()
    {
        if (peekCooldownTimer > 0f)
            peekCooldownTimer -= Time.deltaTime;

        if (!isPeeking)
            return;

        peekTimer -= Time.deltaTime;

        if (peekTimer <= 0f)
        {
            isPeeking = false;
            peekCooldownTimer = peekCooldown;

            // Drop fries back down
            if (rawFries != null)
                rawFries.transform.position = rawFriesStartPosition;
        }
    }

    private void OnMouseUp()
    {
        if (!isDragging) return;

        isDragging = false;

        // Snap fries back if drag wasn't far enough
        if (rawFries != null && !finished)
            rawFries.transform.position = rawFriesStartPosition;
    }

    private Vector3 GetMouseWorldPos()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = Camera.main.WorldToScreenPoint(transform.position).z;
        return Camera.main.ScreenToWorldPoint(mousePos);
    }

    private void OnMouseDown()
    {
        if (finished) return;

        if (!startedFrying)
        {
            StartFrying();
        }

        // Begin tracking drag
        isDragging = true;
        dragStartWorldPos = GetMouseWorldPos();
        rawFriesDragStartPos = rawFries != null ? rawFries.transform.position : Vector3.zero;

        ShakePan();
        ShakeRawFries();
    }

    private void UpdateDrag()
    {
        if (!isDragging)
            return;

        if (!Input.GetMouseButton(0))
        {
            isDragging = false;

            if (rawFries != null && !finished)
                rawFries.transform.position = rawFriesStartPosition;

            return;
        }

        Vector3 currentMousePos = GetMouseWorldPos();
        float dragDelta = currentMousePos.y - dragStartWorldPos.y;

        if (dragDelta > 0f && fryTimer >= minCookTimeBeforeDrag)
        {
            if (rawFries != null)
                rawFries.transform.position = rawFriesDragStartPos + new Vector3(0f, dragDelta, 0f);

            Debug.Log($"Drag delta: {dragDelta} / threshold: {dragUpThreshold}");

            if (dragDelta >= dragUpThreshold)
            {
                isDragging = false;
                FinishFrying(fryTimer >= burnTime);
            }
        }
    }

    private void StartFrying()
    {
        startedFrying = true;

        if (fryingSound != null)
        {
            fryingSound.Play();
        }
    }

    private void ShakeRawFries()
    {
        if (rawFries == null)
            return;

        rawShakeTimer = rawShakeDuration;
    }

    private void ShakePan()
    {
        panShakeTimer = panShakeDuration;
    }

    private void UpdatePanShake()
    {
        if (panShakeTimer <= 0f)
            return;

        panShakeTimer -= Time.deltaTime;

        float xOffset = Random.Range(-panShakeAmount, panShakeAmount);
        float yOffset = Random.Range(-panShakeAmount, panShakeAmount);
        float rotation = Random.Range(-panShakeRotation, panShakeRotation);

        transform.position = panStartPosition + new Vector3(xOffset, yOffset, 0f);
        transform.rotation = panStartRotation * Quaternion.Euler(0f, 0f, rotation);

        if (panShakeTimer <= 0f)
        {
            transform.position = panStartPosition;
            transform.rotation = panStartRotation;
        }
    }

    private void UpdateRawFriesShake()
    {
        if (rawFries == null || rawShakeTimer <= 0f)
            return;

        rawShakeTimer -= Time.deltaTime;

        float xOffset = Random.Range(-rawShakeAmount, rawShakeAmount);
        float yOffset = Random.Range(-rawShakeAmount, rawShakeAmount);
        float rotation = Random.Range(-shuffleRotation, shuffleRotation);

        rawFries.transform.position = rawFriesStartPosition + new Vector3(xOffset, yOffset, 0f);
        rawFries.transform.rotation = Quaternion.Euler(0f, 0f, rotation);

        if (rawShakeTimer <= 0f)
        {
            rawFries.transform.position = rawFriesStartPosition;
            rawFries.transform.rotation = Quaternion.identity;
        }
    }

    private void UpdateFriesColor()
    {
        if (rawFriesRenderer == null)
            return;

        if (fryTimer < cookedTime)
        {
            float cookProgress = Mathf.Clamp01(fryTimer / cookedTime);
            rawFriesRenderer.color = Color.Lerp(rawColor, cookedColor, cookProgress);
            return;
        }

        float burnWindow = Mathf.Max(0.01f, burnTime - cookedTime);
        float burnProgress = Mathf.Clamp01((fryTimer - cookedTime) / burnWindow);
        rawFriesRenderer.color = Color.Lerp(cookedColor, burnedColor, burnProgress);
    }

    private void FinishFrying(bool burned)
    {
        finished = true;

        if (fryingSound != null)
            fryingSound.Stop();

        Color finalColor = burned ? burnedColor : cookedColor;

        if (rawFries != null)
            rawFries.SetActive(false);

        transform.position = panStartPosition;
        transform.rotation = panStartRotation;

        if (cookedFries != null)
        {
            cookedFries.SetActive(true);
            cookedFries.transform.position = cookedFriesStartPosition;
            cookedFries.transform.rotation = Quaternion.identity;

            // Always grab directly from cookedFries to be safe
            SpriteRenderer cookedRenderer = cookedFries.GetComponent<SpriteRenderer>();
            if (cookedRenderer != null)
                cookedRenderer.color = finalColor;
        }

        // Also apply to friesRenderer as fallback
        if (friesRenderer != null)
            friesRenderer.color = finalColor;

        if (OrderManager.Instance == null)
        {
            Debug.LogError("FryFriesMiniGame: OrderManager instance is missing.");
            return;
        }

        if (!OrderManager.Instance.CompleteFriesFryingForActiveReceipt(burned))
        {
            Debug.Log("Fries were fried, but the active receipt did not accept the cooking step.");
            return;
        }

        StartCoroutine(ReturnToKitchen());
    }

    private IEnumerator ReturnToKitchen()
    {
        yield return new WaitForSeconds(1f);

        string sceneToLoad = !string.IsNullOrEmpty(returnSceneName)
            ? returnSceneName
            : OrderManager.Instance.kitchenSceneName;

        if (!Application.CanStreamedLevelBeLoaded(sceneToLoad))
        {
            Debug.LogError("Cannot load scene: " + sceneToLoad + ". Check that it is added to Build Settings and the name is correct.");
            yield break;
        }

        OrderManager.Instance.PrepareForSceneLoad();
        SceneManager.LoadScene(sceneToLoad);
    }
}
