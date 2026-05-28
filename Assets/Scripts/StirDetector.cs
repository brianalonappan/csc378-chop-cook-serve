using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class StirDetector : MonoBehaviour
{
    [SerializeField] private float requiredAngle = 300f;
    [SerializeField] private int trailLength = 30;
    [SerializeField] private Transform bowlCenter;
    [SerializeField] private Transform spoon;
    [SerializeField] private Transform mixtureToRotate;
    [SerializeField] private Camera inputCamera;
    [SerializeField] private float minimumDistanceFromCenter = 0.15f;
    [SerializeField] private UnityEvent onStirComplete = new UnityEvent();

    private readonly Queue<Vector2> inputTrail = new Queue<Vector2>();
    private float totalAngleSwiped;
    private float lastAngle;
    private bool isDragging;

    public UnityEvent OnStirComplete => onStirComplete;

    private void Awake()
    {
        if (inputCamera == null)
            inputCamera = Camera.main;

        if (bowlCenter == null)
            bowlCenter = transform;

        if (mixtureToRotate == null)
            mixtureToRotate = transform;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && !PointerIsOverIngredient())
            StartDrag();

        if (Input.GetMouseButton(0) && isDragging)
            ContinueDrag();

        if (Input.GetMouseButtonUp(0))
            EndDrag();
    }

    private void StartDrag()
    {
        Vector2 position = GetPointerWorldPosition();

        isDragging = true;
        totalAngleSwiped = 0f;
        inputTrail.Clear();
        AddTrailPosition(position);
        lastAngle = GetAngle(position);
    }

    private void ContinueDrag()
    {
        Vector2 position = GetPointerWorldPosition();
        Vector2 center = bowlCenter.position;

        if (Vector2.Distance(position, center) < minimumDistanceFromCenter)
            return;

        float angle = GetAngle(position);
        float delta = Mathf.DeltaAngle(lastAngle, angle);

        totalAngleSwiped += Mathf.Abs(delta);
        lastAngle = angle;
        AddTrailPosition(position);

        if (mixtureToRotate != null)
            mixtureToRotate.Rotate(0f, 0f, delta);

        if (spoon != null)
        {
            spoon.position = position;
            spoon.rotation = Quaternion.Euler(0f, 0f, angle - 90f);
        }

        if (totalAngleSwiped >= requiredAngle)
        {
            totalAngleSwiped = 0f;
            onStirComplete?.Invoke();
        }
    }

    private void EndDrag()
    {
        isDragging = false;
        inputTrail.Clear();
    }

    private Vector2 GetPointerWorldPosition()
    {
        if (inputCamera == null)
            inputCamera = Camera.main;

        Vector3 worldPosition = inputCamera.ScreenToWorldPoint(Input.mousePosition);
        return new Vector2(worldPosition.x, worldPosition.y);
    }

    private bool PointerIsOverIngredient()
    {
        Vector2 position = GetPointerWorldPosition();
        Collider2D hit = Physics2D.OverlapPoint(position);

        if (hit == null)
            return false;

        return hit.GetComponent<IngredientClickTarget>() != null;
    }

    private float GetAngle(Vector2 position)
    {
        Vector2 center = bowlCenter.position;
        return Mathf.Atan2(position.y - center.y, position.x - center.x) * Mathf.Rad2Deg;
    }

    private void AddTrailPosition(Vector2 position)
    {
        inputTrail.Enqueue(position);

        while (inputTrail.Count > trailLength)
            inputTrail.Dequeue();
    }
}
