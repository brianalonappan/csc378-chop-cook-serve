using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PizzaOvenBakeMiniGame : MonoBehaviour
{
    public float goodStartTime = 3f;
    public float burnTime = 6f;
    public string returnSceneName = "UpDown";

    public TMP_Text statusText;
    public TMP_Text timerText;
    public GameObject rawVisual;
    public GameObject cookedVisual;
    public SpriteRenderer rawRenderer;
    public SpriteRenderer cookedRenderer;
    public Collider2D dragCollider;
    public Color rawColor = Color.white;
    public Color cookedColor = new Color(0.95f, 0.75f, 0.28f, 1f);
    public Color burnedColor = new Color(0.18f, 0.12f, 0.07f, 1f);
    public float dragUpThreshold = 1.5f;

    private float bakeTimer;
    private bool finished;
    private bool showingCookedVisual;
    private bool isDragging;
    private Vector3 dragStartWorldPosition;
    private Vector3 activeVisualStartPosition;
    private SpriteRenderer[] rawRenderers;
    private SpriteRenderer[] cookedRenderers;

    private void Start()
    {
        bakeTimer = 0f;
        finished = false;
        showingCookedVisual = false;

        if (rawRenderer == null && rawVisual != null)
            rawRenderer = rawVisual.GetComponent<SpriteRenderer>();

        if (cookedRenderer == null && cookedVisual != null)
            cookedRenderer = cookedVisual.GetComponent<SpriteRenderer>();

        if (dragCollider == null)
            dragCollider = GetComponent<Collider2D>();

        if (rawVisual != null)
            rawRenderers = rawVisual.GetComponentsInChildren<SpriteRenderer>(true);

        if (cookedVisual != null)
            cookedRenderers = cookedVisual.GetComponentsInChildren<SpriteRenderer>(true);

        SetVisualState(raw: true, cooked: false);

        ResetVisualColors();

        UpdateStatusText("Baking...");
    }

    private void Update()
    {
        if (finished)
            return;

        bakeTimer += Time.deltaTime;
        UpdateTimerText();
        UpdateBakeVisuals();

        if (Input.GetMouseButtonDown(0))
            TryStartDrag();

        if (Input.GetKeyDown(KeyCode.Space))
            TryTakePizzaOut();

        UpdateDrag();
    }

    private void OnMouseDown()
    {
        TryStartDrag();
    }

    private void TryStartDrag()
    {
        if (finished)
            return;

        if (bakeTimer < goodStartTime)
        {
            UpdateStatusText("Too early");
            return;
        }

        if (!PointerIsOverPizza())
            return;

        ShowCookedVisual();
        isDragging = true;
        dragStartWorldPosition = GetMouseWorldPosition();
        activeVisualStartPosition = cookedVisual != null ? cookedVisual.transform.position : transform.position;
    }

    private void OnMouseUp()
    {
        if (!isDragging)
            return;

        isDragging = false;

        if (cookedVisual != null && !finished)
            cookedVisual.transform.position = activeVisualStartPosition;
    }

    public void TryTakePizzaOut()
    {
        if (finished)
            return;

        if (bakeTimer < goodStartTime)
        {
            UpdateStatusText("Too early");
            return;
        }

        finished = true;

        if (bakeTimer >= burnTime)
        {
            FinishPizza(burned: true);
            UpdateStatusText("Burned");
            StartCoroutine(ReturnToKitchenAfterDelay(1.25f, completeOvenStep: false));
            return;
        }

        FinishPizza(burned: false);
        UpdateStatusText("Perfect");
        StartCoroutine(ReturnToKitchenAfterDelay(0.75f, completeOvenStep: true));
    }

    private IEnumerator ReturnToKitchenAfterDelay(float delay, bool completeOvenStep)
    {
        yield return new WaitForSeconds(delay);

        if (completeOvenStep && OrderManager.Instance != null)
            OrderManager.Instance.TryUseStation(StationType.Oven);

        if (OrderManager.Instance != null)
            OrderManager.Instance.PrepareForSceneLoad();

        SceneManager.LoadScene(returnSceneName);
    }

    private void UpdateBakeVisuals()
    {
        if (bakeTimer >= burnTime)
        {
            ShowCookedVisual();
            ApplyCookedBurnColor();
            UpdateStatusText("Burning");
            return;
        }

        if (bakeTimer >= goodStartTime)
        {
            ShowCookedVisual();
            ApplyCookedBurnColor();
            UpdateStatusText("Ready");
            return;
        }
    }

    private void SetVisualState(bool raw, bool cooked)
    {
        SetVisualActive(rawVisual, rawRenderers, raw);

        SetVisualActive(cookedVisual, cookedRenderers, cooked);
    }

    private void ShowCookedVisual()
    {
        if (showingCookedVisual)
            return;

        showingCookedVisual = true;
        SetVisualState(raw: false, cooked: true);

        if (cookedRenderer == null && cookedVisual != null)
            cookedRenderer = cookedVisual.GetComponent<SpriteRenderer>();

        if (cookedRenderer != null)
            cookedRenderer.color = cookedColor;
    }

    private void ApplyCookedBurnColor()
    {
        if (cookedRenderers == null || cookedRenderers.Length == 0)
            return;

        float burnWindow = Mathf.Max(0.01f, burnTime - goodStartTime);
        float burnProgress = Mathf.Clamp01((bakeTimer - goodStartTime) / burnWindow);
        SetCookedColor(Color.Lerp(cookedColor, burnedColor, burnProgress));
    }

    private void FinishPizza(bool burned)
    {
        Color finalColor = burned ? burnedColor : cookedColor;

        SetVisualState(raw: false, cooked: true);

        if (cookedRenderer == null && cookedVisual != null)
            cookedRenderer = cookedVisual.GetComponent<SpriteRenderer>();

        SetCookedColor(finalColor);
    }

    private void ResetVisualColors()
    {
        if (rawRenderer != null)
            rawRenderer.color = rawColor;

        if (cookedRenderer != null)
            cookedRenderer.color = cookedColor;

        SetCookedColor(cookedColor);
    }

    private void UpdateDrag()
    {
        if (!isDragging)
            return;

        if (!Input.GetMouseButton(0))
        {
            isDragging = false;

            if (cookedVisual != null && !finished)
                cookedVisual.transform.position = activeVisualStartPosition;

            return;
        }

        Vector3 currentMousePosition = GetMouseWorldPosition();
        float dragDelta = currentMousePosition.y - dragStartWorldPosition.y;

        if (dragDelta <= 0f)
            return;

        if (cookedVisual != null)
            cookedVisual.transform.position = activeVisualStartPosition + new Vector3(0f, dragDelta, 0f);

        if (dragDelta >= dragUpThreshold)
        {
            isDragging = false;
            TryTakePizzaOut();
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

    private bool PointerIsOverPizza()
    {
        Vector3 mouseWorldPosition = GetMouseWorldPosition();

        if (dragCollider != null && dragCollider.OverlapPoint(mouseWorldPosition))
            return true;

        Collider2D activeCollider = null;

        if (showingCookedVisual && cookedVisual != null)
            activeCollider = cookedVisual.GetComponent<Collider2D>();
        else if (rawVisual != null)
            activeCollider = rawVisual.GetComponent<Collider2D>();

        return activeCollider == null || activeCollider.OverlapPoint(mouseWorldPosition);
    }

    private void SetVisualActive(GameObject visual, SpriteRenderer[] renderers, bool active)
    {
        if (visual == null)
            return;

        if (visual == gameObject)
        {
            if (renderers == null)
                return;

            for (int i = 0; i < renderers.Length; i++)
                renderers[i].enabled = active;

            return;
        }

        visual.SetActive(active);
    }

    private void SetCookedColor(Color color)
    {
        if (cookedRenderers == null || cookedRenderers.Length == 0)
        {
            if (cookedRenderer != null)
                cookedRenderer.color = color;

            return;
        }

        for (int i = 0; i < cookedRenderers.Length; i++)
            cookedRenderers[i].color = color;
    }

    private void UpdateTimerText()
    {
        if (timerText == null)
            return;

        timerText.text = bakeTimer.ToString("0.0") + "s";
    }

    private void UpdateStatusText(string message)
    {
        if (statusText != null)
            statusText.text = message;
    }
}
