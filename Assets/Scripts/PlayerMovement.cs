using UnityEngine;
// Code Gathered by: https://youtu.be/PAA_lCutsfE?si=lfrVQ088btVhCK3I
public class PlayerMovement : MonoBehaviour
{
    public int speed = 2;
    private Rigidbody2D characterBody;
    private Vector2 velocity;
    private Vector2 inputMovement;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        velocity = new Vector2(speed, speed);
        characterBody = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        inputMovement = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        );

        if (inputMovement.x > 0)
        {
            spriteRenderer.flipX = false;
        }
        else if (inputMovement.x < 0)
        {
            spriteRenderer.flipX = true;
        }

        animator.SetFloat("X", inputMovement.x);
        animator.SetFloat("Y", inputMovement.y);
    }

    private void FixedUpdate()
    {
        Vector2 delta = inputMovement * velocity * Time.deltaTime;
        Vector2 newPosition = characterBody.position + delta;
        characterBody.MovePosition(newPosition);
    }
}
