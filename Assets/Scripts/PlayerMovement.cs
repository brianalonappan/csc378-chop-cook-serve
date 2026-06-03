using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 2f;
    public float runSpeedMultiplier = 1.75f;
    public float maxRunTime = 0.75f;
    public float runRechargeTime = 1.5f;
    public float interactDistance = 0.5f;
    public LayerMask interactLayer;

    private Rigidbody2D characterBody;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private Vector2 inputMovement;
    private Vector2 lastMoveDirection = Vector2.down;
    private float runTimeRemaining;

    void Start()
    {
        characterBody = GetComponent<Rigidbody2D>();

        animator = GetComponent<Animator>();

        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        runTimeRemaining = maxRunTime;
    }

    void Update()
    {
        inputMovement = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        );

        if (inputMovement != Vector2.zero)
        {
            lastMoveDirection = inputMovement.normalized;
        }

        animator.SetFloat("X", inputMovement.x);
        animator.SetFloat("Y", inputMovement.y);

        if (inputMovement.x < 0)
        {
            spriteRenderer.flipX = true;
        }
        else if (inputMovement.x > 0)
        {
            spriteRenderer.flipX = false;
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("E was pressed");
            TryInteract();
        }
    }

    void FixedUpdate()
    {
        bool isTryingToRun = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        bool canRun = isTryingToRun && inputMovement != Vector2.zero && runTimeRemaining > 0f;
        float currentSpeed = canRun ? speed * runSpeedMultiplier : speed;

        if (canRun)
        {
            runTimeRemaining = Mathf.Max(0f, runTimeRemaining - Time.fixedDeltaTime);
        }
        else if (!isTryingToRun && runTimeRemaining < maxRunTime)
        {
            runTimeRemaining = Mathf.Min(maxRunTime, runTimeRemaining + Time.fixedDeltaTime / runRechargeTime * maxRunTime);
        }

        Vector2 delta = inputMovement * currentSpeed * Time.fixedDeltaTime;

        Vector2 newPosition = characterBody.position + delta;

        characterBody.MovePosition(newPosition);
    }

    void TryInteract()
    {
        Debug.Log("E was pressed. Checking for interactable station...");

        RaycastHit2D hit = Physics2D.Raycast(
            characterBody.position,
            lastMoveDirection,
            interactDistance,
            interactLayer
        );

        Debug.DrawRay(
            characterBody.position,
            lastMoveDirection * interactDistance,
            Color.red,
            1f
        );

        if (hit.collider == null)
        {
            Debug.Log("No station was hit.");
            return;
        }

        Debug.Log("Station Interacted With: " + hit.collider.name);

        InteractableScript station = hit.collider.GetComponent<InteractableScript>();

        if (station == null)
        {
            Debug.Log("Hit object does not have StationInteractable attached.");
            return;
        }
        station.Interact(transform.position);
    }
}
