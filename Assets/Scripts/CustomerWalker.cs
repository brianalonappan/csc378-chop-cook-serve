using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomerWalker : MonoBehaviour
{
    [Header("Main Stop Points")]
    public Transform entrancePoint;
    public Transform registerPoint;
    public Transform waitPoint;
    public Transform exitPoint;
    public Transform pickupPoint;

    [Header("Optional Pass-Through Routes")]
    public Transform[] routeToRegister;
    public Transform[] routeToWaitPoint;
    public Transform[] routeFromWaitToPickup;
    public Transform[] routeFromPickupToExit;

    [Header("Auto Route Setup")]
    public bool autoFindCustomerSpots = true;
    public string customerSpotsParentName = "CustomerWalkingSpots";

    [Header("Movement")]
    public float moveSpeed = 2f;
    public float stopDistance = 0.08f;
    public float counterPauseSeconds = 0.5f;
    public float pickupPauseSeconds = 0.5f;

    [Header("Animation Names")]
    public string walkRightAnimation = "c1walkright";
    public string walkUpAnimation = "c1walkup";
    public string walkDownAnimation = "c1walkdown";

    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private readonly Queue<Vector3> path = new Queue<Vector3>();
    private Vector3 currentTarget;
    private bool hasTarget;
    private bool isPaused;
    private CustomerDestination finalDestination;
    private string currentAnimation = "";

    public bool reachedRegister;
    public bool waitingForFood;

    private Coroutine activeSequence;

    private enum CustomerDestination
    {
        None,
        Register,
        WaitPoint,
        PickupPoint,
        Exit
    }

    private void Awake()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (autoFindCustomerSpots)
            AutoWireCustomerSpots();
    }

    private void Update()
    {
        if (isPaused)
            return;

        if (!hasTarget)
        {
            PlayIdle();
            return;
        }

        Vector3 beforeMove = transform.position;

        transform.position = Vector3.MoveTowards(
            transform.position,
            currentTarget,
            moveSpeed * Time.deltaTime
        );

        Vector3 moveDirection = transform.position - beforeMove;

        if (moveDirection.sqrMagnitude > 0.000001f)
            PlayWalkAnimation(moveDirection);

        if (Vector3.Distance(transform.position, currentTarget) <= stopDistance)
        {
            transform.position = currentTarget;
            GoToNextPathPointOrArrive();
        }
    }

    private void AutoWireCustomerSpots()
    {
        GameObject parentObject = GameObject.Find(customerSpotsParentName);

        if (parentObject == null)
        {
            Debug.LogWarning("Could not find customer spots parent named: " + customerSpotsParentName);
            return;
        }

        Transform parent = parentObject.transform;

        if (entrancePoint == null)
            entrancePoint = FindChild(parent, "CustomerEntrancePoint");

        if (registerPoint == null)
            registerPoint = FindChild(parent, "CustomerRegisterPoint");

        if (waitPoint == null)
            waitPoint = FindChild(parent, "CustomerWaitPoint");

        if (exitPoint == null)
            exitPoint = FindChild(parent, "CustomerExitPoint");

        if (pickupPoint == null)
            pickupPoint = FindChild(parent, "CustomerPickUpPoint");

        if (exitPoint == null)
            exitPoint = entrancePoint;

        if (routeToRegister == null || routeToRegister.Length == 0)
        {
            routeToRegister = CleanRoute(new Transform[]
            {
                FindChild(parent, "CustomerSpot9"),
                registerPoint
            });
        }

        if (routeToWaitPoint == null || routeToWaitPoint.Length == 0)
        {
            routeToWaitPoint = CleanRoute(new Transform[]
            {
                FindChild(parent, "CustomerSpot3"),
                FindChild(parent, "CustomerSpot4"),
                FindChild(parent, "CustomerSpot5"),
                waitPoint
            });
        }

        if (routeFromWaitToPickup == null || routeFromWaitToPickup.Length == 0)
        {
            routeFromWaitToPickup = CleanRoute(new Transform[]
            {
                FindChild(parent, "CustomerSpot5"),
                FindChild(parent, "CustomerSpot4"),
                FindChild(parent, "CustomerSpot3"),
                pickupPoint
            });
        }

        if (routeFromPickupToExit == null || routeFromPickupToExit.Length == 0)
        {
            routeFromPickupToExit = CleanRoute(new Transform[]
            {
                FindChild(parent, "CustomerSpot9"),
                exitPoint
            });
        }
    }

    private Transform FindChild(Transform parent, string childName)
    {
        if (parent == null)
            return null;

        foreach (Transform child in parent)
        {
            if (child.name == childName)
                return child;
        }

        return null;
    }

    private Transform[] CleanRoute(Transform[] route)
    {
        List<Transform> cleanRoute = new List<Transform>();

        foreach (Transform point in route)
        {
            if (point != null && !cleanRoute.Contains(point))
                cleanRoute.Add(point);
        }

        return cleanRoute.ToArray();
    }

    private void StopActiveSequence()
    {
        if (activeSequence != null)
        {
            StopCoroutine(activeSequence);
            activeSequence = null;
        }
    }

    private void BuildRoute(Transform[] route, Transform fallbackDestination, CustomerDestination destinationType)
    {
        isPaused = false;
        finalDestination = destinationType;
        path.Clear();

        if (route != null && route.Length > 0)
        {
            foreach (Transform point in route)
            {
                if (point == null)
                    continue;

                if (Vector3.Distance(transform.position, point.position) > stopDistance)
                    path.Enqueue(point.position);
            }
        }
        else if (fallbackDestination != null)
        {
            path.Enqueue(fallbackDestination.position);
        }

        GoToNextPathPointOrArrive();
    }

    private void GoToNextPathPointOrArrive()
    {
        if (path.Count > 0)
        {
            currentTarget = path.Dequeue();
            hasTarget = true;
            return;
        }

        hasTarget = false;
        StartCoroutine(ArriveAtFinalTarget());
    }

    private IEnumerator ArriveAtFinalTarget()
    {
        PlayIdle();

        if (finalDestination == CustomerDestination.Register)
        {
            isPaused = true;
            yield return new WaitForSeconds(counterPauseSeconds);
            isPaused = false;

            reachedRegister = true;
            waitingForFood = false;
            Debug.Log("Customer reached register.");
        }
        else if (finalDestination == CustomerDestination.WaitPoint)
        {
            reachedRegister = false;
            waitingForFood = true;
            Debug.Log("Customer waiting for food.");
        }
        else if (finalDestination == CustomerDestination.PickupPoint)
        {
            isPaused = true;
            yield return new WaitForSeconds(pickupPauseSeconds);
            isPaused = false;

            Debug.Log("Customer picked up food.");
        }
        else if (finalDestination == CustomerDestination.Exit)
        {
            reachedRegister = false;
            waitingForFood = false;
            Debug.Log("Customer left restaurant.");
            gameObject.SetActive(false);
        }
    }

    public void StartNewCustomer()
    {
        StopActiveSequence();

        gameObject.SetActive(true);

        if (autoFindCustomerSpots)
            AutoWireCustomerSpots();

        if (entrancePoint != null)
            transform.position = entrancePoint.position;

        reachedRegister = false;
        waitingForFood = false;

        BuildRoute(routeToRegister, registerPoint, CustomerDestination.Register);
    }

    public void WalkToWaitPoint()
    {
        StopActiveSequence();

        reachedRegister = false;
        waitingForFood = false;

        BuildRoute(routeToWaitPoint, waitPoint, CustomerDestination.WaitPoint);
    }

    public void LeaveRestaurant()
    {
        StopActiveSequence();

        reachedRegister = false;
        waitingForFood = false;

        activeSequence = StartCoroutine(WalkToPickupThenExit());
    }

    private IEnumerator WalkToPickupThenExit()
    {
        BuildRoute(routeFromWaitToPickup, pickupPoint, CustomerDestination.PickupPoint);

        while (hasTarget || isPaused)
            yield return null;

        Transform fallbackExit = exitPoint;

        if (fallbackExit == null)
            fallbackExit = entrancePoint;

        BuildRoute(routeFromPickupToExit, fallbackExit, CustomerDestination.Exit);

        activeSequence = null;
    }

    public void RestoreWaitingCustomer()
    {
        StopActiveSequence();

        gameObject.SetActive(true);

        if (autoFindCustomerSpots)
            AutoWireCustomerSpots();

        if (waitPoint != null)
            transform.position = waitPoint.position;

        reachedRegister = false;
        waitingForFood = true;

        path.Clear();
        hasTarget = false;
        isPaused = false;
        PlayIdle();
    }

    public bool IsBusyWithCustomer()
    {
        return reachedRegister || waitingForFood || hasTarget || isPaused || activeSequence != null;
    }

    private void PlayWalkAnimation(Vector3 direction)
    {
        if (animator != null)
            animator.speed = 1f;

        float absX = Mathf.Abs(direction.x);
        float absY = Mathf.Abs(direction.y);

        if (absX > absY)
        {
            PlayAnimation(walkRightAnimation);

            if (spriteRenderer != null)
            {
                // Walking left = same animation, mirrored.
                spriteRenderer.flipX = direction.x < 0f;
            }
        }
        else
        {
            if (spriteRenderer != null)
                spriteRenderer.flipX = false;

            if (direction.y > 0f)
                PlayAnimation(walkUpAnimation);
            else
                PlayAnimation(walkDownAnimation);
        }
    }

    private void PlayIdle()
    {
        if (animator != null)
            animator.speed = 0f;
    }

    private void PlayAnimation(string stateName)
    {
        if (animator == null || string.IsNullOrEmpty(stateName))
            return;

        if (!HasAnimatorState(stateName))
        {
            Debug.LogWarning("Missing animator state: " + stateName);
            return;
        }

        if (currentAnimation == stateName && animator.speed > 0f)
            return;

        currentAnimation = stateName;
        animator.speed = 1f;
        animator.Play(stateName, 0, 0f);
    }

    private bool HasAnimatorState(string stateName)
    {
        if (animator == null || string.IsNullOrEmpty(stateName))
            return false;

        int hash = Animator.StringToHash(stateName);
        return animator.HasState(0, hash);
    }
}