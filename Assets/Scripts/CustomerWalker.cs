using UnityEngine;

public class CustomerWalker : MonoBehaviour
{
    public Transform entrancePoint;
    public Transform registerPoint;
    public Transform waitPoint;

    public float moveSpeed = 2f;
    public float stopDistance = 0.08f;

    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Transform currentTarget;

    public bool reachedRegister;
    public bool waitingForFood;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (currentTarget == null)
        {
            PlayIdle();
            return;
        }

        transform.position = Vector3.MoveTowards(
            transform.position,
            currentTarget.position,
            moveSpeed * Time.deltaTime
        );

        Vector3 direction = currentTarget.position - transform.position;
        PlayWalkAnimation(direction);

        if (Vector3.Distance(transform.position, currentTarget.position) <= stopDistance)
        {
            transform.position = currentTarget.position;
            ArriveAtTarget();
        }
    }

    private void ArriveAtTarget()
    {
        PlayIdle();

        if (currentTarget == registerPoint)
        {
            reachedRegister = true;
            Debug.Log("Customer reached register.");
        }
        else if (currentTarget == waitPoint)
        {
            waitingForFood = true;
            Debug.Log("Customer waiting for food.");
        }
        else if (currentTarget == entrancePoint)
        {
            Debug.Log("Customer left restaurant.");
            gameObject.SetActive(false);
        }

        currentTarget = null;
    }

    public void StartNewCustomer()
    {
        gameObject.SetActive(true);
        transform.position = entrancePoint.position;

        reachedRegister = false;
        waitingForFood = false;

        currentTarget = registerPoint;
    }

    public void WalkToWaitPoint()
    {
        reachedRegister = false;
        waitingForFood = false;
        currentTarget = waitPoint;
    }

    public void LeaveRestaurant()
    {
        reachedRegister = false;
        waitingForFood = false;
        currentTarget = entrancePoint;
    }

    public void RestoreWaitingCustomer()
    {
        gameObject.SetActive(true);
        transform.position = waitPoint.position;

        reachedRegister = false;
        waitingForFood = true;
        currentTarget = null;
    }

    private void PlayWalkAnimation(Vector3 direction)
    {
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            animator.Play("c1walkright");

            if (direction.x < 0)
                spriteRenderer.flipX = true;
            else
                spriteRenderer.flipX = false;
        }
        else
        {
            spriteRenderer.flipX = false;

            if (direction.y > 0)
                animator.Play("c1walkup");
            else
                animator.Play("c1walkdown");
        }
    }

    private void PlayIdle()
    {
        // Later you can replace this with real idle animations.
    }
}