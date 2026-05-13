using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 2f;
    public float interactDistance = 1f;
    public LayerMask interactLayer;

    private Rigidbody2D characterBody;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private Vector2 inputMovement;
    private Vector2 lastMoveDirection = Vector2.down;

    void Start()
    {
        characterBody = GetComponent<Rigidbody2D>();

        animator = GetComponent<Animator>();

        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
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
        Vector2 delta = inputMovement * speed * Time.fixedDeltaTime;

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
        station.Interact();
    }
}