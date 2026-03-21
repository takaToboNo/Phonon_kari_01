using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5.0f;
    [SerializeField] private float jumpForce = 10.0f;
    [SerializeField] private float groundCheckRadius = 0.1f;

    private Rigidbody2D rb;
    private BoxCollider2D col;
    private float horizontalInput;
    private bool isGrounded;
    private float colliderHalfHeight;

    [SerializeField] private LayerMask groundLayer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<BoxCollider2D>();
        colliderHalfHeight = col.bounds.extents.y;
    }

    void Update()
    {
        // ç∂âEà⁄ìÆ
        if (Keyboard.current.aKey.isPressed)
        {
            horizontalInput = -1f;
        }
        else if (Keyboard.current.dKey.isPressed)
        {
            horizontalInput = 1f;
        }
        else
        {
            horizontalInput = 0f;
        }

        Vector2 groundCheckPos = new Vector2(
            transform.position.x,
            transform.position.y - colliderHalfHeight
        );

        // ê⁄ínîªíË
        isGrounded = Physics2D.OverlapCircle(groundCheckPos, groundCheckRadius, groundLayer);

        if(Keyboard.current.spaceKey.wasPressedThisFrame && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
    }

    void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);
    }
}